using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GondrLib.Dependencies
{
    [DefaultExecutionOrder(-10)]
    public class Injector : MonoBehaviour
    {
        private const BindingFlags _BindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        private readonly Dictionary<Type, object> _registry = new Dictionary<Type, object>();

        public static Injector Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            IEnumerable<IDependencyProvider> providers = FindMonoBehaviours().OfType<IDependencyProvider>();
            foreach (IDependencyProvider pro in providers)
            {
                RegisterProvider(pro);
            }
            
            IEnumerable<MonoBehaviour> injectables = FindMonoBehaviours().Where(IsInjectable);
            foreach (var mono in injectables)
            {
                Inject(mono);
            }
        }

        public static void InjectInto(MonoBehaviour mono)
        {
            if (Instance != null)
            {
                Instance.Inject(mono);
            }
            else
            {
                Debug.LogWarning("인젝터가 아직 초기화되지 않았습니다.");
            }
        }

        private void Inject(MonoBehaviour mono)
        {
            Type type = mono.GetType();
            
            IEnumerable<FieldInfo> injectableFields = type.GetFields(_BindingFlags)
                .Where(f => Attribute.IsDefined(f, typeof(InjectAttribute)));

            foreach (var field in injectableFields)
            {
                Type fieldType = field.FieldType;
                object instance = ResolveType(fieldType);
                Debug.Assert(instance != null, $"레지스트리에서 주입할 인스턴스를 찾을 수 없습니다 : {fieldType.Name}");
                
                field.SetValue(mono, instance);
            }
            
            IEnumerable<MethodInfo> injectableMethods = type.GetMethods(_BindingFlags)
                .Where(f => Attribute.IsDefined(f, typeof(InjectAttribute)));

            foreach (var method in injectableMethods)
            {
                Type[] requireParam = method.GetParameters()
                    .Select(p => p.ParameterType).ToArray();
                object[] paramValues = requireParam.Select(ResolveType).ToArray();
                method.Invoke(mono, paramValues);
            }
        }

        private object ResolveType(Type type)
        {
            _registry.TryGetValue(type, out object instance);
            return instance;
        }

        private bool IsInjectable(MonoBehaviour mono)
        {
            MemberInfo[] members = mono.GetType().GetMembers(_BindingFlags);
            return members.Any(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
        }

        private void RegisterProvider(IDependencyProvider pro)
        {
            if(Attribute.IsDefined(pro.GetType(), typeof(ProvideAttribute)))
            {
                _registry.TryAdd(pro.GetType(), pro);
                return;
            }
            
            MethodInfo[] methods = pro.GetType().GetMethods(_BindingFlags);

            foreach (var method in methods)
            {
                if(!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;
                
                Type returnType = method.ReturnType;
                object returnInstance = method.Invoke(pro, null);
                Debug.Assert(returnInstance != null, $"Provide 메서드가 void를 반환했습니다 {method.Name}");
                
                _registry.TryAdd(returnType, returnInstance);
            }
        }

        private IEnumerable<MonoBehaviour> FindMonoBehaviours()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }
    }
}