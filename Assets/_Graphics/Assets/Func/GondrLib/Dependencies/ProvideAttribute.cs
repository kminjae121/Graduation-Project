using System;

namespace GondrLib.Dependencies
{
    //의존성을 제공하는 녀석들은 매서드 또는 클래스에 붙일 수 있어.
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ProvideAttribute : Attribute
    {
        
    }
}