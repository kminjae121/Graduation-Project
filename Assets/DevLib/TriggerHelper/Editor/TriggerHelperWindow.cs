using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace DevLib.TriggerHelper.Editor
{
    public class TriggerHelperWindow : EditorWindow
    {
        //등록한 에셋을 저장하는 변수들.
        private GameObject _modelPrefab;
        private AnimationClip _animationClip;
        private GameObject _particlePrefab;
        
        //에디터에만 존재하는 프리뷰 씬을 만들기 위한 변수들
        private Scene _previewScene;
        private Camera _previewCamera;
        private RenderTexture _previewRT; //씬 카메라가 렌더링한 결과를 넣어줄 텍스쳐
        private GameObject _modelInstance;
        private GameObject _particleInstance; //파티클과 모델을 생성시에 생성된 인스턴스를 저장할 변수.

        //파티클의 위치를 모델에서 상대적인 위치로 잡기 위한 값
        private Vector3 _particleOffset; //상대 위치
        private Vector3 _particleRotation; //오일러 각으로 저장된 회전치
        
        //하단 타임라인 드래그 상태를 저장할 정보들
        private float _triggerNormalizedTime = 0.1f; //최초 시작시에 10% 위치에 놓는다.
        private float _currentNormalizedTime = 0.1f; //현재 재생지점의 정규화된 값.
        private bool _isDraggingTrigger; //트리거를 드래그 하고 있는가? (주황색)
        private bool _isDraggingCurrent; //현재 재생지점을 드래그 하고 있는가? (하얀색)
        
        //재생관련 현재 상태 정보들
        private bool _isPlaying; //재생중인지 확인
        private double _playStartTime; //재생시작시간.
        private float _playbackSpeed = 1f; //재생속도
        
        //UI툴킷 참조변수들
        private Label _triggerLabel;
        private IMGUIContainer _timelineContainer;
        private Image _previewImage;
        private VisualElement _gizmoOverlay; //기즈모를 표현할 비쥬얼 엘레멘트
        private VisualElement _previewArea; //프리뷰 이미지를 넣을 영역
        private Label _previewPlaceholder; //프리뷰가 없을 때 표시할 라벨.
        private Label _offsetXLabel, _offsetYLabel, _offsetZLabel; //위치 표기 라벨
        private Label _rotXLabel, _rotYLabel, _rotZLabel; //회전 표기 라벨
        private Label _speedLabel;
            
        
        [MenuItem("Tools/TriggerHelper")]
        private static void ShowWindow()
        {
            var window = GetWindow<TriggerHelperWindow>();
            window.titleContent = new GUIContent("Trigger helper");
            window.minSize = new Vector2(780, 460);
            window.Show();
        }

        private void CreateGUI()
        {
            if (!LoadUxmlAsset()) return; //UXML 로드 실패시 리턴.
            
            FindElementAndBindCallbacks();        
        }

        private bool LoadUxmlAsset()
        {
            string directory = GetEditorDirectory();
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{directory}/TriggerHelperWindow.uxml");
            if (uxml == null)
            {
                rootVisualElement.Add(new Label($"[Trigger Helper] UXML not found: {directory}"));
                return false;
            }
            uxml.CloneTree(rootVisualElement);
            return true;
        }

        /// <summary>
        /// uxml에서 요소를 찾아오고 모델필드, 클립필드, 파티클 필드, 슬라이더 등 UXML에 이벤트를 바인딩
        /// </summary>
        private void FindElementAndBindCallbacks()
        {
            //프리뷰 이미지와 홀더.
            _previewArea = rootVisualElement.Q<VisualElement>("PreviewArea");
            _previewArea.RegisterCallback<GeometryChangedEvent>(_ => { if (_modelInstance != null) RenderPreviewCamera(); });
            _previewPlaceholder = rootVisualElement.Q<Label>("PreviewPlaceholder");
            _previewImage = rootVisualElement.Q<Image>("PreviewImage");
            _previewImage.scaleMode = ScaleMode.StretchToFill;
            _gizmoOverlay = rootVisualElement.Q<VisualElement>("GizmoOverlay");
            _gizmoOverlay.generateVisualContent += DrawGizmoContent;
            
            ObjectField modelField = rootVisualElement.Q<ObjectField>("ModelField");
            modelField.objectType = typeof(GameObject);
            modelField.allowSceneObjects = false;
            modelField.RegisterValueChangedCallback(e => OnModelChange(e.newValue as GameObject));
            
            ObjectField animationClipField = rootVisualElement.Q<ObjectField>("ClipField");
            animationClipField.objectType = typeof(AnimationClip);
            animationClipField.allowSceneObjects = false;
            animationClipField.RegisterValueChangedCallback(e => OnAnimationClipChange(e.newValue as AnimationClip));
            
            ObjectField particlePrefabField = rootVisualElement.Q<ObjectField>("ParticleField");
            particlePrefabField.allowSceneObjects = false;
            particlePrefabField.objectType = typeof(GameObject);
            particlePrefabField.RegisterValueChangedCallback(e => OnParticleChange(e.newValue as GameObject));
            
            _speedLabel = rootVisualElement.Q<Label>("SpeedLabel");
            Slider speedSlider = rootVisualElement.Q<Slider>("SpeedSlider");
            if (speedSlider != null) //스피드 슬라이더 변경시 재생속도 변경.
            {
                speedSlider.RegisterValueChangedCallback(e =>
                {
                    _playbackSpeed = e.newValue;
                    _speedLabel.text = $"x {e.newValue:F2}"; //배속 표시

                    if (_isPlaying) //현재 재생중이였다면 타이머를 리셋해서 올바르게 재생시간을 작성한다.
                    {
                        _playStartTime = EditorApplication.timeSinceStartup 
                                         - _currentNormalizedTime * _animationClip.length / _playbackSpeed;
                    }
                });
            }

            rootVisualElement.Q<Button>("PlayBtn").clicked += StartPlay;
            rootVisualElement.Q<Button>("StopBtn").clicked += StopPlay;
            
            _offsetXLabel = rootVisualElement.Q<Label>("OffsetXLabel");
            _offsetYLabel = rootVisualElement.Q<Label>("OffsetYLabel");
            _offsetZLabel = rootVisualElement.Q<Label>("OffsetZLabel");

            WireOffsetSlider(rootVisualElement.Q<Slider>("OffsetXSlider"), _offsetXLabel, v => { _particleOffset.x = v; OnOffsetChanged(); });
            WireOffsetSlider(rootVisualElement.Q<Slider>("OffsetYSlider"), _offsetYLabel, v => { _particleOffset.y = v; OnOffsetChanged(); });
            WireOffsetSlider(rootVisualElement.Q<Slider>("OffsetZSlider"), _offsetZLabel, v => { _particleOffset.z = v; OnOffsetChanged(); });

            rootVisualElement.Q<Button>("CopyPosBtn").clicked += CopyPosition;

            _rotXLabel = rootVisualElement.Q<Label>("RotXLabel");
            _rotYLabel = rootVisualElement.Q<Label>("RotYLabel");
            _rotZLabel = rootVisualElement.Q<Label>("RotZLabel");

            WireOffsetSlider(rootVisualElement.Q<Slider>("RotXSlider"), _rotXLabel, v => { _particleRotation.x = v; OnOffsetChanged(); });
            WireOffsetSlider(rootVisualElement.Q<Slider>("RotYSlider"), _rotYLabel, v => { _particleRotation.y = v; OnOffsetChanged(); });
            WireOffsetSlider(rootVisualElement.Q<Slider>("RotZSlider"), _rotZLabel, v => { _particleRotation.z = v; OnOffsetChanged(); });

            rootVisualElement.Q<Button>("CopyRotBtn").clicked += CopyRotation;

            _triggerLabel = rootVisualElement.Q<Label>("TriggerTimeLabel");
            _timelineContainer = rootVisualElement.Q<IMGUIContainer>("TimelineContainer");
            _timelineContainer.onGUIHandler = DrawTimelineGUI;

            RefreshTriggerLabel();
        }

        #region Playback handling

        private void StartPlay()
        {
            if (_animationClip == null || _modelInstance == null) return;
            if(_isPlaying) StopPlay(); //정지

            _currentNormalizedTime = 0f;
            _isPlaying = true;
            _playStartTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += HandleEditorUpdate;
        }

        private void StopPlay()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            EditorApplication.update -= HandleEditorUpdate;
        }

        private void HandleEditorUpdate()
        {
            //재생 중이지 않을 때 들어왔거나 클립을 빼버렸다면 바로 정지 Safe Code
            if(!_isPlaying || _animationClip == null) { 
                StopPlay();
                return;
            }
            
            double elapsed = (EditorApplication.timeSinceStartup - _playStartTime) * _playbackSpeed;
            _currentNormalizedTime = Mathf.Clamp01( (float)(elapsed / _animationClip.length));
            
            SampleAt(_currentNormalizedTime);
            Repaint();
            
            if(_currentNormalizedTime >= 1f) StopPlay();
        }

        #endregion
        
        #region Ui toolkit callbacks
        
        private void OnModelChange(GameObject newValue)
        {
            _modelPrefab = newValue;
            RebuildInstances();
            RenderPreviewCamera();
        }

        private void OnAnimationClipChange(AnimationClip newValue)
        {
            _animationClip = newValue;
            _currentNormalizedTime = 0f;
            SampleAt(_currentNormalizedTime);
            Repaint(); //다음 프레임에 윈도우를 새로 그리라는 명령(유니티 매서드)
        }

        private void OnParticleChange(GameObject newValue)
        {
            if (newValue != null && newValue.GetComponentInChildren<ParticleSystem>() == null)
                Debug.LogWarning("[TriggerHelper] 등록된 프리팹에 ParticleSystem 컴포넌트가 없습니다.");
            _particlePrefab = newValue;
            RebuildInstances();
        }

        private void OnOffsetChanged()
        {
            SampleAt(_currentNormalizedTime);
            Repaint();
        }
        
        private static void WireOffsetSlider(Slider slider, Label valueLabel, System.Action<float> onChange)
        {
            if (slider == null) return;
            slider.RegisterValueChangedCallback(e =>
            {
                valueLabel.text = $"{e.newValue:F2}";
                onChange(e.newValue);
            });
        }
        
        private void CopyPosition()
        {
            string text = $"({_particleOffset.x:F3}, {_particleOffset.y:F3}, {_particleOffset.z:F3})";
            EditorGUIUtility.systemCopyBuffer = text;
            ShowNotification(new GUIContent($"Position 복사됨\n{text}"));
        }

        private void CopyRotation()
        {
            string text = $"({_particleRotation.x:F1}, {_particleRotation.y:F1}, {_particleRotation.z:F1})";
            EditorGUIUtility.systemCopyBuffer = text;
            ShowNotification(new GUIContent($"Rotation 복사됨\n{text}"));
        }
        
        #endregion

        #region Rendering image

        private void RenderPreviewCamera()
        {
            if (!_previewScene.IsValid() || _previewCamera == null || _modelInstance == null) return;
            if (_previewArea == null || _previewImage == null) return;
            
            Rect cr = _previewArea.contentRect;
            int w = Mathf.Max(1, (int)cr.width);
            int h = Mathf.Max(1, (int)cr.height);
            if (w < 4 || h < 4) return;

            if (_previewRT == null || _previewRT.width != w || _previewRT.height != h)
            {
                if (_previewRT != null) _previewRT.Release();
                _previewRT = new RenderTexture(w, h, 16, RenderTextureFormat.ARGB32);
                _previewImage.image = _previewRT;
            }

            _previewCamera.targetTexture = _previewRT;
            _previewCamera.Render();
            _previewCamera.targetTexture = null;

            _previewImage.MarkDirtyRepaint();
            _gizmoOverlay.MarkDirtyRepaint();
        }
        
        /// <summary>
        ///애니메이션과 파티클 시스템의 특정 지점으로 샘플링해서 이미지로 가져온다. 
        /// </summary>
        /// <param name="normalizedTime">0~1까지의 정규화된 값. - 전체 클립길이 대비 샘플링할 위치</param>
        private void SampleAt(float normalizedTime)
        {
            if(_animationClip == null || _modelInstance == null) return;
            
            float t = normalizedTime * _animationClip.length;
            //애니메이션 클립에 모델을 넣고 샘플링함. (자동으로 해당 오브젝트의 animator를 찾아서 동작)
            _animationClip.SampleAnimation(_modelInstance, t);

            if (_particleInstance != null)
            {
                _particleInstance.transform.SetPositionAndRotation(
                    _modelInstance.transform.position + _particleOffset,
                    _modelInstance.transform.rotation * Quaternion.Euler(_particleRotation));
                
                ParticleSystem ps = _particleInstance.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    float elapsed = t - _triggerNormalizedTime * _animationClip.length;
                    if (elapsed >= 0f)
                    {
                        ps.Simulate(elapsed, true, true); //해당 지점을 재생하도록 
                    }
                    else
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);//존재하는 파티클도 제거
                        ps.Clear(true);
                    }
                }
            }
            
            RenderPreviewCamera();
        }
        
        private void DrawGizmoContent(MeshGenerationContext context)
        {
            if (_previewCamera == null || _modelInstance == null) return;

            var rect = _gizmoOverlay.contentRect;
            Vector3 vp = _previewCamera.WorldToViewportPoint(
                _modelInstance.transform.position + _particleOffset);

            if (vp.z <= 0f || vp.x < -0.05f || vp.x > 1.05f || vp.y < -0.05f || vp.y > 1.05f)
                return;

            float px = vp.x * rect.width;
            float py = (1f - vp.y) * rect.height;

            Color c = _particleInstance != null
                ? new Color(1f, 0.85f, 0f, 0.92f)
                : new Color(0.65f, 0.65f, 0.65f, 0.55f);

            var painter = context.painter2D;
            painter.lineWidth = 2f;
            painter.strokeColor = c;

            const float arm = 11f;
            const float gap = 3.5f;

            // Horizontal arms
            painter.BeginPath();
            painter.MoveTo(new Vector2(px - arm - gap, py));
            painter.LineTo(new Vector2(px - gap, py));
            painter.Stroke();

            painter.BeginPath();
            painter.MoveTo(new Vector2(px + gap, py));
            painter.LineTo(new Vector2(px + arm + gap, py));
            painter.Stroke();

            // Vertical arms
            painter.BeginPath();
            painter.MoveTo(new Vector2(px, py - arm - gap));
            painter.LineTo(new Vector2(px, py - gap));
            painter.Stroke();

            painter.BeginPath();
            painter.MoveTo(new Vector2(px, py + gap));
            painter.LineTo(new Vector2(px, py + arm + gap));
            painter.Stroke();

            // Center dot
            painter.fillColor = c;
            painter.BeginPath();
            painter.Arc(new Vector2(px, py), 3.5f, 0f, 360f);
            painter.Fill();
        }
        
        #endregion

        #region IMGUI rendering

        private void DrawTimelineGUI()
        {
            Rect contentRect = _timelineContainer.contentRect;
            if (contentRect.width < 4f || contentRect.height < 4f) return; //너무 작으면 그리지 마.
            DrawTimeline(new Rect(0, 0, contentRect.width, contentRect.height)); //0,0위치에 Rect크기로 그려.
        }

        private void DrawTimeline(Rect rect)
        {
            Event currentEvent = Event.current; 
            
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f)); //회색으로 지정된 영역에 배경그림을 깔아줍니다.
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, height: 1f), new Color(0.06f, 0.06f, 0.06f)); //상단 경계선

            const float paddingX = 22f; //x축 패딩
            float trackLeft = rect.x + paddingX;
            float trackRight = rect.xMax - paddingX;
            float trackWidth = trackRight - trackLeft;
            float midY = rect.y + rect.height * 0.48f; 
            //적당히 중간쯤. 0.2를 남겨 놓는 이유는 재생 막대기가 이보다 크게 그려질꺼라서

            const float trackHeight = 4f;
            const float markerHalfWidth = 7f;
            
            //회색의 트랙을 그려준다.
            EditorGUI.DrawRect(new Rect(trackLeft, midY - trackHeight * 0.5f, trackWidth, trackHeight), new Color(0.27f, 0.27f, 0.27f));

            if (_animationClip == null)
            {
                GUI.Label(rect, "Animation clip을 등록해야 트랙을 조절할 수 있습니다.", new GUIStyle(EditorStyles.centeredGreyMiniLabel));
                return;
            }

            float triggerX = trackLeft + _triggerNormalizedTime * trackWidth; //트리거 위치
            float currentX = trackLeft + _currentNormalizedTime * trackWidth; //재생 위치
            Color triggerColor = new Color(1f, 0.5f, 0f);

            if (_currentNormalizedTime > 0f) //재생 막대기가 차올라 가는 것을 그린다.
            {
                EditorGUI.DrawRect(new Rect(trackLeft, midY - trackHeight * 0.5f, currentX - trackLeft, trackHeight), new Color(0.5f, 0.5f, 0.5f));;
            }
            if (_currentNormalizedTime > _triggerNormalizedTime) //트리거가 지나가면 재생막대를 노란색으로 그려준다.
            {
                EditorGUI.DrawRect(new Rect(triggerX, midY - trackHeight * 0.5f, currentX - triggerX, trackHeight), new Color(1f, 0.5f, 0f, 0.8f));
            }
            
            //트리거 막대 그리기 (오렌지색)
            float triggerTime = _triggerNormalizedTime * _animationClip.length;
            int triggerFrame = Mathf.FloorToInt(triggerTime * _animationClip.frameRate);
            DrawMarkerRect(triggerX, midY - 7f, markerHalfWidth, 14f, triggerColor);
            DrawTimeLabel($"◆ {triggerTime:F2}s, frame : {triggerFrame}", triggerX, rect.y + 3f, triggerColor);
            
            //현재 마커 그리기(흰색)
            DrawMarkerRect(currentX, midY + 7f, markerHalfWidth, 14f, Color.white);
            DrawTimeLabel($"◆ {_currentNormalizedTime * _animationClip.length:F2}s", currentX, midY + 14f, Color.white);
            
            // 드래그 검출을 위한 힛 Rect
            Rect triggerHit = new Rect(triggerX - markerHalfWidth - 3f, midY - 14f, (markerHalfWidth + 3f) * 2f, 14f);
            Rect currentHit = new Rect(currentX - markerHalfWidth - 3f, midY, (markerHalfWidth + 3f) * 2f, 14f);

            switch (currentEvent.type)
            {
                case EventType.MouseDown when rect.Contains(currentEvent.mousePosition):
                {
                    if (triggerHit.Contains(currentEvent.mousePosition))
                        _isDraggingTrigger = true;
                    else
                    {
                        _currentNormalizedTime = Clamp01Track(currentEvent.mousePosition.x, trackLeft, trackWidth);
                        _isDraggingCurrent = true;
                        StopPlay();
                        SampleAt(_currentNormalizedTime);
                    }
                    currentEvent.Use();
                    break;
                }

                case EventType.MouseDrag when _isDraggingTrigger || _isDraggingCurrent:
                {
                    float n = Clamp01Track(currentEvent.mousePosition.x, trackLeft, trackWidth);
                    if (_isDraggingTrigger)
                    {
                        _triggerNormalizedTime = n;
                        RefreshTriggerLabel();
                        SampleAt(_currentNormalizedTime);
                    }
                    else
                    {
                        _currentNormalizedTime = n;
                        StopPlay();
                        SampleAt(_currentNormalizedTime);
                    }
                    currentEvent.Use();
                    Repaint();
                    break;
                }

                case EventType.MouseUp:
                {
                    _isDraggingTrigger = false;
                    _isDraggingCurrent = false;
                    break;
                }
            }
        }

        private static void DrawMarkerRect(float centerX, float centerY, float halfWidth, float height, Color color)
            => EditorGUI.DrawRect(new Rect(centerX - halfWidth, centerY - height * 0.5f, halfWidth * 2f, height), color);

        private static void DrawTimeLabel(string text, float centerX, float centerY, Color color)
        {
            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = color }
            };
            GUI.Label(new Rect(centerX - 58f, centerY, 116f, 14f), text, style);
        }
        
        private static float Clamp01Track(float mouseX, float trackL, float trackW)
            => Mathf.Clamp01((mouseX - trackL) / trackW);
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// 트리거 라벨에 현재 트리거 위치의 정규화된 값과, 프레임을 표기하는 매서드
        /// </summary>
        private void RefreshTriggerLabel()
        {
            if (_triggerLabel == null) return;
            if (_animationClip == null) { _triggerLabel.text = "트리거: 클립 미등록"; return; }

            float t = _triggerNormalizedTime * _animationClip.length;
            int frame = Mathf.FloorToInt(t * _animationClip.frameRate);
            _triggerLabel.text = $"트리거: {t:F3}s  |  F{frame}";
        }
        
        /// <summary>
        /// 현재 에디터 스크립트가 실행되는 경로(상대경로) 반환
        /// </summary>
        /// <returns>상대경로</returns>
        private string GetEditorDirectory()
        {
            var script = MonoScript.FromScriptableObject(this);
            string path = AssetDatabase.GetAssetPath(script);
            return Path.GetDirectoryName(path)?.Replace('\\', '/');
        }
        
        /// <summary>
        /// 이미 만들어져 있는 모델이나 파티클이 있다면 삭제해주는 매서드
        /// </summary>
        private void DestroyInstances()
        {
            if (_modelInstance != null) { DestroyImmediate(_modelInstance); _modelInstance = null; }
            if (_particleInstance != null) { DestroyImmediate(_particleInstance); _particleInstance = null; }
        }
        
        private void CreatePreviewLight(string goName, Color color, float intensity, Quaternion rotation)
        {
            var go = new GameObject(goName, typeof(Light));
            SceneManager.MoveGameObjectToScene(go, _previewScene);
            var light = go.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1.3f;
            light.transform.rotation = Quaternion.Euler(40f, 45f, 0f);
        }
        
        /// <summary>
        /// 프리뷰씬이 있는지 확인하여 없다면 생성해준다.
        /// </summary>
        private void EnsurePreviewSceneAndCamera()
        {
            if (_previewScene.IsValid()) return;

            // NewPreviewScene 는 기존씬과 완전히 분리된 씬을 만든다. 이것은 하이라키에서 보이지 않는다.
            _previewScene = EditorSceneManager.NewPreviewScene();

            // Camera
            var camGO = new GameObject("__MainCamera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(camGO, _previewScene);
            _previewCamera = camGO.GetComponent<Camera>();
            _previewCamera.backgroundColor = new Color(0.14f, 0.14f, 0.14f, 1f);
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.nearClipPlane = 0.01f;
            _previewCamera.farClipPlane = 300f;
            _previewCamera.cameraType = CameraType.Preview;
            _previewCamera.cullingMask = -1;
            _previewCamera.scene = _previewScene; // render only preview scene objects
            _previewCamera.enabled = false; // manual Render() only

            // Key light
            CreatePreviewLight("__DIR_Light0", Color.white, 1.3f, Quaternion.Euler(40f, 45f, 0f));
            // Fill light
            CreatePreviewLight("__DIR_Light1", new Color(0.55f, 0.6f, 0.8f), 0.45f, Quaternion.identity);
        }

        /// <summary>
        /// 플레이스홀더를 표기할 것인지 결정해주는 매서드
        /// </summary>
        private void UpdatePreviewPlaceholder()
        {
            if (_previewPlaceholder == null) return;
            bool hasModel = _modelInstance != null;
            _previewPlaceholder.style.display = hasModel ? DisplayStyle.None : DisplayStyle.Flex;
        }
        
        /// <summary>
        /// 카메라가 생성된 모델 인스턴스를 바라보도록 셋업.
        /// </summary>
        private void FitCamera()
        {
            if (_previewCamera == null || _modelInstance == null) return;

            var renderers = _modelInstance.GetComponentsInChildren<Renderer>();
            Bounds b;
            if (renderers.Length > 0)
            {
                b = renderers[0].bounds;
                foreach (var r in renderers) b.Encapsulate(r.bounds); //렌더러들을 합쳐서 가장 큰 바운드를 계산
            }
            else
            {
                b = new Bounds(_modelInstance.transform.position + Vector3.up, new Vector3(2f, 2f, 2f));
            }

            float size = Mathf.Max(b.size.x, b.size.y, b.size.z, 0.5f);
            Vector3 center = b.center;

            //모델에 전방에 카메라를 두고 center를 바라보도록 고정
            _previewCamera.transform.position = center + new Vector3(0f, size * 0.3f, size * 2.5f);
            _previewCamera.transform.LookAt(center);
        }
        
        /// <summary>
        /// 모델과 파티클의 인스턴스를 갱신.
        /// </summary>
        private void RebuildInstances()
        {
            DestroyInstances(); //이미 만들어진 모델을 삭제.
            if (_modelPrefab == null)
            {
                UpdatePreviewPlaceholder();
                Repaint();
                return;
            }
            EnsurePreviewSceneAndCamera(); //프리뷰씬의 존재를 확인.(없다면 만들어줌)
            _modelInstance = Instantiate(_modelPrefab); //모델 인스턴스 만들어줌.
            SceneManager.MoveGameObjectToScene(_modelInstance, _previewScene);
            
            FitCamera();
            
            if (_particlePrefab != null)
            {
                _particleInstance = Instantiate(_particlePrefab);
                SceneManager.MoveGameObjectToScene(_particleInstance, _previewScene);
            }
            
            UpdatePreviewPlaceholder();
            SampleAt(_currentNormalizedTime);
            Repaint();
        }
        
        #endregion

        #region Safe code section - cleanup

        /// <summary>
        /// 창 닫기전에 모두 클린업해주는 함수(메모리 안전)
        /// </summary>
        private void CleanupAll()
        {
            StopPlay();
            DestroyInstances();

            if (_previewRT != null) //렌더 텍스쳐 릴리즈
            {
                _previewRT.Release();
                _previewRT = null;
                if (_previewImage != null) _previewImage.image = null;
            }

            if (_previewScene.IsValid()) //프리뷰씬 릴리즈
            {
                EditorSceneManager.ClosePreviewScene(_previewScene);
                _previewScene = default;
                _previewCamera = null;
            }
        }
        private void OnDisable()  => CleanupAll();
        private void OnDestroy()  => CleanupAll();

        #endregion
    }
}