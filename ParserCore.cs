using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Web;
using XMLParser.Models;
using Aspose.Docs.Scrapper;
using Confluence.DataProvider;
using System.Configuration;
using Confluence.ServicesProxy;
using System.Reflection;
//using Aspose.JavaDoc.Reader;
//using Aspose.JavaDoc.JavaReader;
using System.Collections;
using System.ComponentModel;
using XMLParser;
using System.Text.RegularExpressions;
namespace XMLParser
{
    public class ParserCore
    {
       

        public static bool IsOverriding(MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException("methodInfo");
            return methodInfo.DeclaringType != methodInfo.GetBaseDefinition().DeclaringType;
        }

        public static void ExtractXML(APIClassMembers eachProp,IEnumerable<XElement> members,ref ProductAPI AsposeProductAPI,string nameSpace,string className)
        {
            string paramMain = "";

            if(eachProp.lstParams!=null)
            foreach (var eachP in eachProp.lstParams)
            {
                if (paramMain != "")
                    paramMain += ",";

                paramMain += eachP.ParamFullName;

            }

            if (paramMain != "")
            {
             
                string matchText=string.Format("M:{0}.{1}.{2}({3})",nameSpace, className,eachProp.Alias, paramMain);
                var methodOverload = members.SingleOrDefault(s => s.Attribute("name").Value == (matchText));

                if (methodOverload != null)
                    FetchMemberInfo(eachProp, ref AsposeProductAPI, methodOverload);
            }

            else
            {

                string matchText = string.Format("M:{0}.{1}.{2}", nameSpace, className, eachProp.Name, paramMain);
                var methodOverload = members.SingleOrDefault(s => s.Attribute("name").Value == (matchText));
                if (methodOverload != null)
                    FetchMemberInfo(eachProp, ref AsposeProductAPI, methodOverload);
            }

        }
        public static void ExtractXML(APIClassMembers eachProp, IEnumerable<XElement> members, ref ProductAPI AsposeProductAPI, string nameSpace, string className,bool isDuplicated)
        {
            string paramMain = "";

            if (eachProp.lstParams != null)
                foreach (var eachP in eachProp.lstParams)
                {
                    if (paramMain != "")
                        paramMain += ",";

                    paramMain += eachP.ParamFullName;

                }

            if (paramMain != "")
            {

                string matchText = string.Format("M:{0}.{1}.({2})",  className, eachProp.Alias, paramMain);
                var methodOverload = members.SingleOrDefault(s => s.Attribute("name").Value == (matchText));

                if (methodOverload != null)
                    FetchMemberInfo(eachProp, ref AsposeProductAPI, methodOverload);
            }

            else
            {

              
                string matchText = string.Format("M:{0}.{1}.({2})", className, eachProp.Name, paramMain);
                var methodOverload = members.SingleOrDefault(s => s.Attribute("name").Value == (matchText));
                if (methodOverload != null)
                    FetchMemberInfo(eachProp, ref AsposeProductAPI, methodOverload);
            }

        }
        
        public static bool IsHiding(MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException("methodInfo");
            if (methodInfo.DeclaringType == methodInfo.GetBaseDefinition().DeclaringType)
            {
                var baseType = methodInfo.DeclaringType.BaseType;
                if (baseType != null)
                {
                    MethodInfo hiddenBaseMethodInfo = null;
                    var methods = baseType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Static);
                    foreach (var mi in methods)
                        if (mi.Name == methodInfo.Name)
                        {
                            var miParams = mi.GetParameters();
                            var methodInfoParams = methodInfo.GetParameters();
                            if (miParams.Length == methodInfoParams.Length)
                            {
                                var i = 0;
                                for (; i < miParams.Length; i++)
                                {
                                    if (miParams[i].ParameterType != methodInfoParams[i].ParameterType
                                        || ((miParams[i].Attributes ^ methodInfoParams[i].Attributes).HasFlag(ParameterAttributes.Out))) break;

                                    // Simplified from:
                                    //if (miParams[i].ParameterType != methodInfoParams[i].ParameterType
                                    //    || (miParams[i].Attributes.HasFlag(ParameterAttributes.Out) && !methodInfoParams[i].Attributes.HasFlag(ParameterAttributes.Out))
                                    //    || !(miParams[i].Attributes.HasFlag(ParameterAttributes.Out) && methodInfoParams[i].Attributes.HasFlag(ParameterAttributes.Out))) break;
                                }
                                if (i == miParams.Length)
                                {
                                    hiddenBaseMethodInfo = mi;
                                    break;
                                }
                            }
                        }
                    if (hiddenBaseMethodInfo != null && !hiddenBaseMethodInfo.IsPrivate) return true;
                }
            }
            return false;
        }
        public static void AddClassToNameSpace(APIClass Class, string nameSpace, ref ProductAPI API)
        {
            APINameSpace nspace = new APINameSpace();

            nspace.Title = nameSpace;

            if(API.lstNameSpaces!=null)
            foreach (var eachN in API.lstNameSpaces)
            {
                if (eachN.lstClasses != null)
                {
                    var existinClass = eachN.lstClasses.SingleOrDefault(s => s.ClassName == Class.ClassName);

                    if (existinClass != null)
                    {

                        Class.IsDuplicatedInOtherNameSpace = true;
                        Class.ClassName = nameSpace + "." + Class.ClassName;

                        break;
                   }
                    else
                        Class.IsDuplicatedInOtherNameSpace = false;

                       
                }


            }


            if (API.lstNameSpaces.SingleOrDefault(s => s.Title == nameSpace) != null)
            {
                nspace = API.lstNameSpaces.SingleOrDefault(s => s.Title == nameSpace);


                if (Class.Type == Types.ClassType.Structure)
                {
                    if (nspace.StructureCount.HasValue)
                    {
                        nspace.StructureCount = nspace.StructureCount.Value + 1;

                    }
                    else
                    {
                        nspace.StructureCount = 1;
                    }
                }

                if (nspace.lstClasses != null)
                {

                    var existinClass = nspace.lstClasses.SingleOrDefault(s => s.ClassName == Class.ClassName);

                    if (existinClass == null)
                        nspace.AddAPIClass(Class);
                }
                else
                    nspace.AddAPIClass(Class);

            }
            else
            {
                if (Class.Type == Types.ClassType.Structure)
                {
                    if (nspace.StructureCount.HasValue)
                    {
                        nspace.StructureCount = nspace.StructureCount.Value + 1;

                    }
                    else
                    {
                        nspace.StructureCount = 1;
                    }
                }
                nspace.AddAPIClass(Class);
                API.AddNameSpace(nspace);
            }

        }


        public static void IncreaseCount(int Item, string nameSpace,ref ProductAPI API)
        {
            var ns = API.lstNameSpaces.SingleOrDefault(s => s.Title == nameSpace);

            // 1 Interface, 2 Enum, 3 Struct
            if (ns == null) return ;

            switch (Item)
            {

                case 0:
                    if (ns.ClassCount.HasValue)
                        ns.ClassCount++;
                    else
                        ns.ClassCount = 1;
                    break;
                case 1:
                    if (ns.InterfaceCount.HasValue)
                        ns.InterfaceCount++;
                    else
                        ns.InterfaceCount = 1;
                   
                    break;
                case 2:
                    if (ns.EnumCount.HasValue)
                        ns.EnumCount++;
                    else
                        ns.EnumCount = 1;
                    
                    break;

                case 3:
                    if (ns.StructureCount.HasValue)
                        ns.StructureCount++;
                    else
                        ns.StructureCount = 1;
                  
                    break;



            }

        }


        public static bool IfMethodExists(ref ProductAPI API, APIClassMembers member)
        {
            bool isExists = false ;

            if(API.lstNameSpaces!=null)
            foreach (var eachNS in API.lstNameSpaces)
            {

                if (eachNS.lstClasses!= null)
                foreach (var eachC in eachNS.lstClasses)
                {

                    if(eachC.lstMembers!=null)
                        if (eachC.lstMembers.SingleOrDefault(s => s.Name == member.Name) != null)
                        {
                            isExists = true;
                            break;
                        }
                }

            }

            return isExists;


        }




        public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            var derivedType = typeof(T);
            return assembly
                .GetTypes()
                .Where(t =>
                    t != derivedType &&
                    derivedType.IsAssignableFrom(t)
                    ).ToList();

        }

        public static void ParseDllFirst(Assembly LoadedAssembly, ref XMLParser.Models.ProductAPI AsposeProductAPI)
        {
           

          // var allTypes = LoadedAssembly.GetTypes().Where(w => w.IsPublic == true).OrderBy(s => s.Namespace).ThenBy(s => s.Name);

           Type[] allTypes;
           try
           {
               allTypes = LoadedAssembly.GetTypes();
           }
           catch (ReflectionTypeLoadException reflectionTypeLoadException)
           {
               allTypes = reflectionTypeLoadException.Types;
           }

           allTypes = (allTypes.Where(w => w != null && w.IsPublic == true).OrderBy(s => s.Namespace).ThenBy(s => s.Name)).ToArray();

            foreach (var eachType in allTypes)
            {
                if (AsposeProductAPI.SpaceKey.ToLower() == "wordsnet") 
                {
                    if (!eachType.Namespace.Contains("Aspose.Words"))
                        continue;

                    if (eachType.Namespace.Contains("Aspose.Words.Tests"))
                        continue;

                }

             

                try
                {
                    #region DLL Parsing and Hierchies FOrming
                    APIClass objClass = new APIClass();

                    objClass.ClassName = eachType.Name;
                    objClass.NameSpace = eachType.Namespace;
                    objClass.Assembly = AsposeProductAPI.AssemblyName;

                    if (!eachType.IsPublic)
                        continue;

                    object[] attributes = eachType.GetCustomAttributes(false);

                    foreach (FlagsAttribute attribute in attributes.OfType<FlagsAttribute>())
                    {
                        objClass.IsFlagAttribute = true;
                        objClass.flagAttributeMessage = "This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.";

                    }

                    foreach (ObsoleteAttribute attribute in attributes.OfType<ObsoleteAttribute>())
                    {
                        objClass.IsObsolete = true;
                        objClass.obsoleteMessage = attribute.Message;

                    }


                    if (eachType.IsClass && eachType.BaseType.FullName != "System.MulticastDelegate")
                    {
                        #region Class Region


                        #region Class Formation






                        objClass.Attributes = eachType.Attributes.ToString();
                        objClass.IsSealed = eachType.IsSealed;
                        objClass.IsAbstract = eachType.IsAbstract;
                        objClass.IsSerializable = eachType.IsSerializable;


                        if (eachType.BaseType != null)
                        {

                            objClass.AddBaseType(eachType.BaseType.FullName, eachType.BaseType.Name);

                            if (eachType.BaseType.BaseType != null)
                            {
                                objClass.AddBaseType(eachType.BaseType.BaseType.FullName, eachType.BaseType.BaseType.Name);


                                if (eachType.BaseType.BaseType.BaseType != null)
                                {
                                    objClass.AddBaseType(eachType.BaseType.BaseType.BaseType.FullName, eachType.BaseType.BaseType.BaseType.Name);

                                    if (eachType.BaseType.BaseType.BaseType.BaseType != null)
                                        objClass.AddBaseType(eachType.BaseType.BaseType.BaseType.BaseType.FullName, eachType.BaseType.BaseType.BaseType.BaseType.Name);

                                }
                            }

                        }




                        foreach (var eachT in allTypes)
                        {

                            if (eachT.BaseType == eachType)
                            {
                                objClass.AddDerivedType(eachT.Name, eachT.FullName);


                            }

                        }



                        objClass.hasICompareable = typeof(IComparable).IsAssignableFrom(eachType);
                        objClass.hasIDisposable = typeof(IDisposable).IsAssignableFrom(eachType);
                        #endregion

                        try
                        {
                            AddClassToNameSpace(objClass, eachType.Namespace, ref AsposeProductAPI);

                        }

                        catch (Exception ex)
                        {
                            WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                        }


                        //var info = eachType.GetMethods(BindingFlags.Public | BindingFlags.Instance).OrderBy(o => o.Name);

                        //var info2 = eachType.GetMethods().Where(s => s.IsSpecialName == false && s.IsPublic == true);

                        var InfoPublic = eachType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(s => s.IsSpecialName == false && s.IsPublic == true).OrderBy(s => s.Name);


                        //var InfoProtected = eachType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(s => s.IsVirtual == true && s.IsFinal == false);


                        BindingFlags bindingFlags =BindingFlags.Instance |BindingFlags.NonPublic;



                        BindingFlags bindingFlagsStatic = BindingFlags.Static | BindingFlags.Public;

                        var InfoProtected2 = eachType.GetMethods(bindingFlags).OrderBy(s=>s.Name);

                        var InfoStatic = eachType.GetMethods(bindingFlagsStatic).OrderBy(s => s.Name);  


                        if (InfoStatic != null)
                        {
                            try
                            {

                                #region  Methods Formation
                                List<APIClassMembers> classMembers = new List<APIClassMembers>();

                                foreach (var eachMethod in InfoStatic)
                                {


                                    try
                                    {
                                        #region Protected Method Extraction

                                        APIClassMembers mem = new APIClassMembers();
                                        mem.Alias = eachMethod.Name;
                                        mem.Name = eachMethod.Name;
                                        mem.isConstructor = eachMethod.IsConstructor;
                                        mem.IsAbstract = eachMethod.IsAbstract;
                                        mem.IsFinal = eachMethod.IsFinal;
                                        mem.IsGeneric = eachMethod.IsGenericMethod;
                                        mem.IsVitual = eachMethod.IsVirtual;
                                        mem.IsStatic = true;

                                        mem.ReturnType = eachMethod.ReturnType.ToString();

                                        ParameterInfo[] ConParams = eachMethod.GetParameters();

                                        foreach (var eachPara in ConParams)
                                        {
                                            APIClassMembersParameters par = new APIClassMembersParameters();
                                            par.ParamName = eachPara.Name;
                                            par.ParamType = eachPara.ParameterType.Name;
                                            par.IsOut = eachPara.IsOut;
                                            par.IsOptional = eachPara.IsOptional;
                                            par.ParamFullName = eachPara.ParameterType.FullName;



                                            mem.AddMethodParameter(par);

                                        }

                                        List<string> DefaultMethods = new List<string>();

                                        DefaultMethods.Add("ToString");
                                        DefaultMethods.Add("Equals");
                                        DefaultMethods.Add("GetHashCode");
                                        DefaultMethods.Add("GetType");


                                        mem.Signature = ParsingHelper.GetMethodSignature(mem);

                                        if (classMembers.SingleOrDefault(s => s.Name == mem.Name) == null)
                                        {
                                            if (!IfMethodExists(ref AsposeProductAPI, mem) || (DefaultMethods.Contains(mem.Name)))
                                            {
                                                mem.IsDuplicated = false;
                                                classMembers.Add(mem);

                                            }
                                            else
                                            {
                                                mem.IsDuplicated = true;
                                                //  mem.Name = objClass.NameSpace+ objClass.ClassName + "." + mem.Name;
                                                classMembers.Add(mem);

                                            }


                                        }
                                        else
                                        {
                                            var member = classMembers.SingleOrDefault(s => s.Name == mem.Name);
                                            member.AddAPIClassFormat(mem);

                                        }

                                        #endregion

                                    }

                                    catch (Exception ex)
                                    {
                                        WebThreadHelper.UpdateProgress("Error: " + ex.Message);

                                    }
                                }

                                if (objClass.lstMembers == null)
                                    objClass.lstMembers = classMembers;
                                else
                                    objClass.lstMembers = objClass.lstMembers.Union(classMembers).ToList();




                                #endregion


                            }
                            catch (Exception ex)
                            {
                                WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                            }

                        }
                       

                        if (InfoProtected2 != null)
                        {


                            try
                            {

                                #region  Methods Formation
                                List<APIClassMembers> classMembers = new List<APIClassMembers>();

                                foreach (var eachMethod in InfoProtected2)
                                {

                                    if (!ParsingHelper.IsAlphanumeric(eachMethod.Name))
                                        continue;

                                    if (eachMethod.IsSecurityCritical)
                                        continue;

                                    
                                    try
                                    {
                                        #region Protected Method Extraction

                                        APIClassMembers mem = new APIClassMembers();
                                        mem.Alias = eachMethod.Name;
                                        mem.Name = eachMethod.Name;
                                        mem.isConstructor = eachMethod.IsConstructor;
                                        mem.IsAbstract = eachMethod.IsAbstract;
                                        mem.IsFinal = eachMethod.IsFinal;
                                        mem.IsGeneric = eachMethod.IsGenericMethod;
                                        mem.IsVitual = eachMethod.IsVirtual;
                                        mem.IsProtected = true;

                                        mem.ReturnType = eachMethod.ReturnType.ToString();

                                        ParameterInfo[] ConParams = eachMethod.GetParameters();

                                        foreach (var eachPara in ConParams)
                                        {
                                            APIClassMembersParameters par = new APIClassMembersParameters();
                                            par.ParamName = eachPara.Name;
                                            par.ParamType = eachPara.ParameterType.Name;
                                            par.IsOut = eachPara.IsOut;
                                            par.IsOptional = eachPara.IsOptional;
                                            par.ParamFullName = eachPara.ParameterType.FullName;



                                            mem.AddMethodParameter(par);

                                        }

                                        List<string> DefaultMethods = new List<string>();

                                        DefaultMethods.Add("ToString");
                                        DefaultMethods.Add("Equals");
                                        DefaultMethods.Add("GetHashCode");
                                        DefaultMethods.Add("GetType");


                                        mem.Signature = ParsingHelper.GetMethodSignature(mem);

                                        if (classMembers.SingleOrDefault(s => s.Name == mem.Name) == null)
                                        {
                                            if (!IfMethodExists(ref AsposeProductAPI, mem) || (DefaultMethods.Contains(mem.Name)))
                                            {
                                                mem.IsDuplicated = false;
                                                classMembers.Add(mem);

                                            }
                                            else
                                            {
                                                mem.IsDuplicated = true;
                                                //  mem.Name = objClass.NameSpace+ objClass.ClassName + "." + mem.Name;
                                                classMembers.Add(mem);

                                            }


                                        }
                                        else
                                        {
                                            var member = classMembers.SingleOrDefault(s => s.Name == mem.Name);
                                            member.AddAPIClassFormat(mem);

                                        }

                                        #endregion

                                    }

                                    catch (Exception ex)
                                    {
                                        WebThreadHelper.UpdateProgress("Error: " + ex.Message);

                                    }
                                }

                                if (objClass.lstMembers == null)
                                    objClass.lstMembers = classMembers;
                                else
                                    objClass.lstMembers = objClass.lstMembers.Union(classMembers).ToList();


                                #endregion


                            }
                            
                            catch (Exception ex)
                            {
                                WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                            }

                        }

                        if (InfoPublic != null)
                        {
                            try
                            {

                                #region  Methods Formation
                                List<APIClassMembers> classMembers = new List<APIClassMembers>();

                                foreach (var eachMethod in InfoPublic)
                                {
                                    if (!eachMethod.IsPublic)
                                        continue;

                                    try
                                    {
                                        #region Method Extraction

                                        APIClassMembers mem = new APIClassMembers();
                                        mem.Alias = eachMethod.Name;
                                        mem.isHideBySig = eachMethod.IsHideBySig;
                                        if (eachMethod.IsHideBySig)
                                        {

                                            var flags = eachMethod.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
                                            flags |= eachMethod.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
                                            var paramTypes = eachMethod.GetParameters().Select(p => p.ParameterType).ToArray();
                                            if (eachMethod.DeclaringType.BaseType.GetMethod(eachMethod.Name, flags, null, paramTypes, null) != null)
                                            {
                                                // the property's 'get' method shadows by signature
                                                mem.isNew = true;
                                            }


                                        }
                                        mem.Name = eachMethod.Name;
                                        mem.isConstructor = eachMethod.IsConstructor;
                                        mem.IsAbstract = eachMethod.IsAbstract;
                                        mem.IsFinal = eachMethod.IsFinal;
                                        mem.IsGeneric = eachMethod.IsGenericMethod;
                                        mem.IsVitual = eachMethod.IsVirtual;

                                        mem.ReturnType = eachMethod.ReturnType.ToString();

                                        ParameterInfo[] ConParams = eachMethod.GetParameters();

                                        foreach (var eachPara in ConParams)
                                        {
                                            APIClassMembersParameters par = new APIClassMembersParameters();
                                            par.ParamName = eachPara.Name;
                                            par.ParamType = eachPara.ParameterType.Name;
                                            par.IsOut = eachPara.IsOut;
                                            par.IsOptional = eachPara.IsOptional;
                                            par.ParamFullName = eachPara.ParameterType.FullName;

                                       

                                            mem.AddMethodParameter(par);

                                        }

                                        List<string> DefaultMethods = new List<string>();

                                        DefaultMethods.Add("ToString");
                                        DefaultMethods.Add("Equals");
                                        DefaultMethods.Add("GetHashCode");
                                        DefaultMethods.Add("GetType");
                                        

                                        mem.Signature = ParsingHelper.GetMethodSignature(mem);

                                        if (classMembers.SingleOrDefault(s => s.Name == mem.Name) == null)
                                        {
                                            if (!IfMethodExists(ref AsposeProductAPI, mem) || (DefaultMethods.Contains(mem.Name)))
                                            {
                                                mem.IsDuplicated = false;
                                                classMembers.Add(mem);

                                            }
                                            else
                                            {
                                                mem.IsDuplicated = true;
                                                //  mem.Name = objClass.NameSpace+ objClass.ClassName + "." + mem.Name;
                                                classMembers.Add(mem);

                                            }


                                        }
                                        else
                                        {
                                            var member = classMembers.SingleOrDefault(s => s.Name == mem.Name);
                                            member.AddAPIClassFormat(mem);

                                        }

                                        #endregion

                                    }

                                    catch (Exception ex)
                                    {
                                        WebThreadHelper.UpdateProgress("Error: " + ex.Message);

                                    }
                                }

                                if (objClass.lstMembers == null)
                                    objClass.lstMembers = classMembers;
                                else
                                    objClass.lstMembers = objClass.lstMembers.Union(classMembers).ToList();



                                #endregion


                            }
                            catch (Exception ex)
                            {
                                WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                            }
                        }

                        var Events = eachType.GetEvents(BindingFlags.Public | BindingFlags.Instance).OrderBy(s => s.Name);

                        if (Events != null)
                        {

                            foreach (var eachE in Events)
                            {

                                var EvenType = allTypes.SingleOrDefault(s => s.Name == eachE.EventHandlerType.Name);
                                APIEvents EVT = new APIEvents();
                                EVT.Name = eachE.Name;
                                EVT.BaseType = eachE.EventHandlerType.Name;



                                objClass.AddEvents(EVT);
                            }

                        }

                        var meminfo = eachType.GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy(s => s.Name);

                        if (meminfo != null)
                        {
                            try
                            {
                                #region  Fields Formation
                                foreach (var eachMem in meminfo)
                                {
                                    if (!eachMem.IsPublic)
                                        continue;

                                    APIClassProperties mem = new APIClassProperties();

                                    mem.Name = eachMem.Name;
                                    mem.Type = eachMem.FieldType.Name;
                                    mem.TypeFullName = eachMem.FieldType.FullName;
                                    mem.isStatic = eachMem.IsStatic;
                                    mem.IsField = true;

                                    PropertyInfo pi = eachMem.GetType().GetProperty(eachMem.Name);

                                    if (pi != null)
                                    {


                                        if (pi.GetSetMethod() == null)
                                            mem.IsSetter = false;
                                        else
                                            mem.IsSetter = true;

                                        if (pi.GetGetMethod() == null)
                                            mem.ISGetter = false;
                                        else
                                            mem.ISGetter = true;
                                    }

                                    ParsingHelper.AddClassProperty(eachType.Namespace, objClass.ClassName, mem, ref AsposeProductAPI);
                                }
                                #endregion

                                
                            }

                            catch (Exception ex)
                            {
                                WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                            }

                        }

                        var propInfo = eachType.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(s => s.Name);

                        var propInfoStatic = eachType.GetFields(BindingFlags.Public | BindingFlags.Static).OrderBy(s => s.Name);

                        var propInfoProtected = eachType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance ).OrderBy(s => s.Name);

                        if (propInfoProtected != null)
                        {
                            try
                            {
                                #region Static Fields

                                foreach (var eachMem in propInfoProtected)
                                {
                                    if (!ParsingHelper.IsAlphanumeric(eachMem.Name))
                                        continue;


                                    if (eachMem.IsSecurityCritical)
                                        continue;

                                    APIClassProperties mem = new APIClassProperties();

                                    mem.Name = eachMem.Name;
                                    mem.Type = eachMem.FieldType.Name;
                                    mem.TypeFullName = eachMem.FieldType.FullName;
                                    mem.isProtected = true;

                                    mem.IsField = true;
                                    mem.isStatic = false;
                                    mem.IsReadOnly = eachMem.IsInitOnly;
                                    mem.isLiteral = eachMem.IsLiteral;


                                    ParsingHelper.AddClassProperty(eachType.Namespace, objClass.ClassName, mem, ref AsposeProductAPI);
                                }

                                #endregion
                            }

                            catch (Exception ex)
                            {
                                WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                            }
                        }
                        
                        if (propInfoStatic != null)
                        {
                               try
                               {
                                   #region Static Fields 

                                   foreach (var eachMem in propInfoStatic)
                                   {

                                       APIClassProperties mem = new APIClassProperties();

                                       mem.Name = eachMem.Name;
                                       mem.Type = eachMem.FieldType.Name;
                                       mem.TypeFullName = eachMem.FieldType.FullName;

                                       mem.IsField = true ;
                                       mem.isStatic = true;
                                       mem.IsReadOnly = eachMem.IsInitOnly;
                                       mem.isLiteral = eachMem.IsLiteral;

                                      
                                       ParsingHelper.AddClassProperty(eachType.Namespace, objClass.ClassName, mem, ref AsposeProductAPI);
                                   }

                                   #endregion
                               }

                               catch (Exception ex)
                               {
                                   WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                               }
                        }


                        if (propInfo != null)
                        {
                            try
                            {
                                #region  PropertyInfo Formation
                                foreach (var eachMem in propInfo)
                                {
                                   
                                    APIClassProperties mem = new APIClassProperties();


                                         if (eachMem.GetIndexParameters().Length > 0)
                                        {
                                            // Indexed property...
                                            mem.isIndexer = true;
                                            foreach (ParameterInfo pinfo in eachMem.GetIndexParameters())
                                            {

                                                mem.IndexerName = pinfo.Name;
                                                mem.IndexerType = pinfo.ParameterType.Name;

                                            }
                                        }
                                    

                                    mem.Name = eachMem.Name;
                                    mem.Type = eachMem.PropertyType.Name;
                                    mem.TypeFullName = eachMem.PropertyType.FullName;
                                  
                                    mem.IsField = false;

                                    if(eachMem.GetSetMethod()!=null)
                                    mem.IsSetter = eachMem.CanWrite;

                                    if(eachMem.GetGetMethod()!=null)
                                    mem.ISGetter = eachMem.CanRead;

                                    ParsingHelper.AddClassProperty(eachType.Namespace, objClass.ClassName, mem, ref AsposeProductAPI);
                                }
                                #endregion
                            }

                            catch (Exception ex)
                            {
                                WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                            }

                        }

                        ConstructorInfo[] ConInfo = eachType.GetConstructors(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);


                        ConstructorInfo[] ConInfoProtected = eachType.GetConstructors(BindingFlags.NonPublic| BindingFlags.Instance);


                       

                        if (ConInfo != null)
                        {
                            if (ConInfo.Length <= 0)
                                continue;

                            int DefaultConParamLength = 20;

                            try
                            {

                                #region  Constructor  Formation
                                List<APIClassMembers> lstConstructors = new List<APIClassMembers>();

                                foreach (var eachMethod in ConInfo)
                                {
                                    if (!eachMethod.IsPublic)
                                        continue;

                                    APIClassMembers mem = new APIClassMembers();

                                    mem.Name = eachMethod.Name;
                                    mem.isConstructor = eachMethod.IsConstructor;
                                    mem.IsAbstract = eachMethod.IsAbstract;
                                    mem.IsFinal = eachMethod.IsFinal;
                                    mem.IsGeneric = eachMethod.IsGenericMethod;
                                    mem.IsVitual = eachMethod.IsVirtual;
                                    mem.isConstructor = true;
                                    mem.ParamsLength = eachMethod.GetParameters().Length;
                                    mem.IsOverLoad = true;

                             
                                    if (eachMethod.GetParameters().Length < DefaultConParamLength)
                                        DefaultConParamLength = eachMethod.GetParameters().Length;

                                    ParameterInfo[] ConParams = eachMethod.GetParameters();

                                    foreach (var eachPara in ConParams)
                                    {
                                        APIClassMembersParameters par = new APIClassMembersParameters();

                                        par.ParamName = eachPara.Name;
                                        par.ParamType = eachPara.ParameterType.Name;
                                        par.IsOut = eachPara.IsOut;
                                        par.IsOptional = eachPara.IsOptional;
                                        par.ParamFullName = eachPara.ParameterType.FullName;

                                        mem.AddMethodParameter(par);

                                    }

                                    mem.Signature = ParsingHelper.GetMethodSignature(mem);
                                    mem.Signature = mem.Signature.Replace(".ctor", objClass.ClassName);

                                    if (!mem.Signature.Contains("("))
                                        mem.Signature += " ( ) ";

                                    lstConstructors.Add(mem);

                                }

                                APIClassMembers DefaultConstructor = lstConstructors.SingleOrDefault(w => w.ParamsLength == DefaultConParamLength);


                                lstConstructors.Remove(DefaultConstructor);

                                int Counter=1;
                                foreach (var each in lstConstructors)
                                {

                                    each.Name = objClass.ClassName + " Constructor Overload_" + Counter.ToString();
                                    Counter++;
                                }

                                DefaultConstructor.lstOverLoads = lstConstructors;
                                DefaultConstructor.IsOverLoad = false;




                                ParsingHelper.AddClassMembers(eachType.Namespace, objClass.ClassName, DefaultConstructor, ref AsposeProductAPI);

                                #endregion


                            }
                            catch (Exception ex)
                            {
                                WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                            }

                        }

                        if (ConInfoProtected != null)
                        {
                            if (ConInfoProtected.Length <= 0)
                                continue;

                            int DefaultConParamLength = 20;

                            try
                            {

                                #region  Constructor  Formation
                                List<APIClassMembers> lstConstructors = new List<APIClassMembers>();

                                foreach (var eachMethod in ConInfoProtected)
                                {
                                    if (ConInfo.SingleOrDefault(s => s.Name == eachMethod.Name) != null)
                                        continue;

                                    APIClassMembers mem = new APIClassMembers();

                                    mem.Name = eachMethod.Name;
                                    mem.isConstructor = eachMethod.IsConstructor;
                                    mem.IsAbstract = eachMethod.IsAbstract;
                                    mem.IsFinal = eachMethod.IsFinal;
                                    mem.IsGeneric = eachMethod.IsGenericMethod;
                                    mem.IsVitual = eachMethod.IsVirtual;
                                    mem.isConstructor = true;
                                    mem.ParamsLength = eachMethod.GetParameters().Length;
                                    mem.IsOverLoad = true;
                                    mem.IsProtected = true;


                                    if (eachMethod.GetParameters().Length < DefaultConParamLength)
                                        DefaultConParamLength = eachMethod.GetParameters().Length;

                                    ParameterInfo[] ConParams = eachMethod.GetParameters();

                                    foreach (var eachPara in ConParams)
                                    {
                                        APIClassMembersParameters par = new APIClassMembersParameters();

                                        par.ParamName = eachPara.Name;
                                        par.ParamType = eachPara.ParameterType.Name;
                                        par.IsOut = eachPara.IsOut;
                                        par.IsOptional = eachPara.IsOptional;
                                        par.ParamFullName = eachPara.ParameterType.FullName;

                                        mem.AddMethodParameter(par);

                                    }

                                    mem.Signature = ParsingHelper.GetMethodSignature(mem);
                                    mem.Signature = mem.Signature.Replace(".ctor", objClass.ClassName);

                                    if (!mem.Signature.Contains("("))
                                        mem.Signature += " ( ) ";

                                    lstConstructors.Add(mem);

                                }

                                APIClassMembers DefaultConstructor = lstConstructors.SingleOrDefault(w => w.ParamsLength == DefaultConParamLength);


                                lstConstructors.Remove(DefaultConstructor);

                                int Counter = 1;
                                foreach (var each in lstConstructors)
                                {

                                    each.Name = objClass.ClassName + " Constructor Overload_" + Counter.ToString();
                                    Counter++;
                                }

                                DefaultConstructor.lstOverLoads = lstConstructors;
                                DefaultConstructor.IsOverLoad = false;




                                ParsingHelper.AddClassMembers(eachType.Namespace, objClass.ClassName, DefaultConstructor, ref AsposeProductAPI);

                                #endregion


                            }
                            catch (Exception ex)
                            {
                                WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                            }

                        }
                        #endregion


                        IncreaseCount(0, eachType.Namespace, ref AsposeProductAPI);
                    }

                    else
                    {

                        if (eachType.IsEnum && eachType.IsValueType)
                        {

                            MemberInfo[] memberInfos = eachType.GetMembers(BindingFlags.Public | BindingFlags.Static);
                            FieldInfo[] infos = eachType.GetFields(BindingFlags.Public | BindingFlags.Static);

                         

                            foreach (FieldInfo eachM in infos)
                          {
                               object o= eachM.GetRawConstantValue();
                               APIEnumTypes type = new APIEnumTypes();

                               type.Name = eachM.Name;
                                if(eachM.GetRawConstantValue()!=null)
                               type.Val = eachM.GetRawConstantValue().ToString();

                                objClass.AddEnumType(type);

                              
                            }

                            #region Enumeration
                            objClass.Type = Types.ClassType.Enumeration;


                            try
                            {
                                AddClassToNameSpace(objClass, eachType.Namespace, ref AsposeProductAPI);

                            }

                            catch (Exception ex)
                            {
                                WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                            }

                            #endregion


                            IncreaseCount(2, eachType.Namespace, ref AsposeProductAPI);
                        }

                        if (eachType.IsInterface)
                        {

                            objClass.Type = Types.ClassType.Interface;

                            try
                            {
                                AddClassToNameSpace(objClass, eachType.Namespace, ref AsposeProductAPI);

                            }

                            catch (Exception ex)
                            {
                                WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                            }

                            var propInfo = eachType.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(s => s.Name);


                            if (propInfo != null)
                            {
                                try
                                {
                                    #region  PropertyInfo Formation
                                    foreach (var eachMem in propInfo)
                                    {

                                        APIClassProperties mem = new APIClassProperties();

                                        mem.Name = eachMem.Name;
                                        mem.Type = eachMem.PropertyType.Name;
                                        mem.TypeFullName = eachMem.PropertyType.FullName;


                                        mem.IsField = false;

                                        if (eachMem.GetSetMethod() != null)
                                            mem.IsSetter = eachMem.CanWrite;

                                        if (eachMem.GetGetMethod() != null)
                                            mem.ISGetter = eachMem.CanRead;

                                        ParsingHelper.AddClassProperty(eachType.Namespace, objClass.ClassName, mem, ref AsposeProductAPI);
                                    }
                                    #endregion
                                }

                                catch (Exception ex)
                                {
                                    WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                                }

                            }

                            var InfoPublic = eachType.GetMethods().OrderBy(s => s.Name);

                        if (InfoPublic != null)
                            {
                                try
                                {

                                    #region  Methods Formation
                                    List<APIClassMembers> classMembers = new List<APIClassMembers>();

                                    foreach (var eachMethod in InfoPublic)
                                    
                                    {
                                        if (!eachMethod.IsPublic)
                                            continue;

                                        APIClassMembers mem = new APIClassMembers();

                                        mem.Name = eachMethod.Name;
                                        mem.isConstructor = eachMethod.IsConstructor;
                                        mem.IsAbstract = eachMethod.IsAbstract;
                                        mem.IsFinal = eachMethod.IsFinal;
                                        mem.IsGeneric = eachMethod.IsGenericMethod;
                                        mem.IsVitual = eachMethod.IsVirtual;
                                        mem.ReturnType = eachMethod.ReturnType.ToString();

                                        ParameterInfo[] ConParams = eachMethod.GetParameters();

                                        foreach (var eachPara in ConParams)
                                        {
                                            APIClassMembersParameters par = new APIClassMembersParameters();
                                            par.ParamName = eachPara.Name;
                                            par.ParamType = eachPara.ParameterType.Name;
                                            par.IsOut = eachPara.IsOut;
                                            par.IsOptional = eachPara.IsOptional;

                                            mem.AddMethodParameter(par);

                                        }

                                        mem.Signature = ParsingHelper.GetMethodSignature(mem);

                                        if (classMembers.SingleOrDefault(s => s.Name == mem.Name) == null)
                                            classMembers.Add(mem);
                                        else
                                        {
                                            var member = classMembers.SingleOrDefault(s => s.Name == mem.Name);
                                            member.AddAPIClassFormat(mem);

                                        }
                                    }

                                    objClass.lstMembers = classMembers;



                                    #endregion


                                }
                                catch (Exception ex)
                                {
                                    WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                                }
                            }
                          


                            IncreaseCount(1, eachType.Namespace, ref AsposeProductAPI);

                        }

                        if (eachType.IsValueType && !eachType.IsEnum)
                        {

                            objClass.Type = Types.ClassType.Structure;
                            #region Structure

                            try
                            {
                                AddClassToNameSpace(objClass, eachType.Namespace, ref AsposeProductAPI);

                            }

                            catch (Exception ex)
                            {
                                WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                            }

                            var InfoPublic = eachType.GetMethods(BindingFlags.Public | BindingFlags.Static).OrderBy(s => s.Name);

                            if (InfoPublic != null)
                            {
                                try
                                {

                                    #region  Methods Formation
                                    List<APIClassMembers> classMembers = new List<APIClassMembers>();

                                    foreach (var eachMethod in InfoPublic)
                                    {
                                        if (!eachMethod.IsPublic)
                                            continue;

                                        APIClassMembers mem = new APIClassMembers();

                                        mem.Name = eachMethod.Name;

                                        mem.IsAbstract = eachMethod.IsAbstract;
                                        mem.IsFinal = eachMethod.IsFinal;
                                        mem.IsGeneric = eachMethod.IsGenericMethod;
                                        mem.IsVitual = eachMethod.IsVirtual;
                                        mem.ReturnType = eachMethod.ReturnType.ToString();
                                        mem.IsStatic = true;

                                       
                                        ParameterInfo[] ConParams = eachMethod.GetParameters();

                                        foreach (var eachPara in ConParams)
                                        {
                                            APIClassMembersParameters par = new APIClassMembersParameters();
                                            par.ParamName = eachPara.Name;
                                            par.ParamType = eachPara.ParameterType.Name;
                                            par.IsOut = eachPara.IsOut;
                                            par.IsOptional = eachPara.IsOptional;

                                            mem.AddMethodParameter(par);

                                        }

                                        if (classMembers.SingleOrDefault(s => s.Name == mem.Name) == null)
                                            classMembers.Add(mem);
                                        else
                                        {
                                            var member = classMembers.SingleOrDefault(s => s.Name == mem.Name);
                                            member.AddAPIClassFormat(mem);

                                        }
                                        mem.Signature = ParsingHelper.GetMethodSignature(mem);

                                    }

                                 

                                    objClass.lstMembers = classMembers;



                                    #endregion



                                }
                                catch (Exception ex)
                                {
                                    WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                                }
                            }

                            var meminfo = eachType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).OrderBy(s => s.Name);

                            if (meminfo != null)
                            {
                                try
                                {
                                    #region  Fields Formation
                                    foreach (var eachMem in meminfo)
                                    {
                                        if (!eachMem.IsPublic)
                                            continue;

                                        APIClassProperties mem = new APIClassProperties();

                                        mem.Name = eachMem.Name;
                                        mem.Type = eachMem.FieldType.Name;
                                        mem.TypeFullName = eachMem.FieldType.FullName;
                                        mem.isStatic = eachMem.IsStatic;



                                        ParsingHelper.AddClassProperty(eachType.Namespace, objClass.ClassName, mem, ref AsposeProductAPI);
                                    }
                                    #endregion
                                }

                                catch (Exception ex)
                                {
                                    WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                                }

                            }
                            #endregion




                            IncreaseCount(3, eachType.Namespace, ref AsposeProductAPI);

                        }

                        if (eachType.IsClass && eachType.BaseType.FullName == "System.MulticastDelegate")
                        {
                            try
                            {

                                MethodInfo method = eachType.GetMethod("Invoke");

                                string ReturnType = method.ReturnType.Name;

                                objClass.Type = Types.ClassType.Delegate;

                                DelegateInfo INfo = new DelegateInfo();

                                INfo.ReturnType = ReturnType;
                                INfo.ReturnTypeFullName = method.ReturnType.FullName;

                                foreach (ParameterInfo param in method.GetParameters())
                                {
                                    //.// Console.WriteLine("{0} {1}", param.ParameterType.Name, param.Name);
                                    INfo.AddParam(param.Name, param.ParameterType.FullName, param.ParameterType.Name);
                                }

                                objClass.DelegateInfo = INfo;

                                try
                                {
                                    AddClassToNameSpace(objClass, eachType.Namespace, ref AsposeProductAPI);

                                }

                                catch (Exception ex)
                                {
                                    WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                                }
                            }

                            catch (Exception ex)
                            {

                                WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                            }


                        }

                        if (eachType.IsClass && eachType.BaseType.Name.Contains("EventArgs"))
                        {
                            #region Class Region



                            objClass.Attributes = eachType.Attributes.ToString();
                            objClass.IsSealed = eachType.IsSealed;
                            objClass.IsAbstract = eachType.IsAbstract;
                            objClass.IsSerializable = eachType.IsSerializable;


                            if (eachType.BaseType != null)
                            {

                                objClass.AddBaseType(eachType.BaseType.FullName, eachType.BaseType.Name);

                                if (eachType.BaseType.BaseType != null)
                                {
                                    objClass.AddBaseType(eachType.BaseType.BaseType.FullName, eachType.BaseType.BaseType.Name);


                                    if (eachType.BaseType.BaseType.BaseType != null)
                                    {
                                        objClass.AddBaseType(eachType.BaseType.BaseType.BaseType.FullName, eachType.BaseType.BaseType.BaseType.Name);

                                        if (eachType.BaseType.BaseType.BaseType.BaseType != null)
                                            objClass.AddBaseType(eachType.BaseType.BaseType.BaseType.BaseType.FullName, eachType.BaseType.BaseType.BaseType.BaseType.Name);

                                    }
                                }

                            }




                            foreach (var eachT in allTypes)
                            {

                                if (eachT.BaseType == eachType)
                                {
                                    objClass.AddDerivedType(eachT.Name, eachT.FullName);


                                }

                            }

                            objClass.hasIDisposable = typeof(IDisposable).IsAssignableFrom(eachType);
                            #endregion

                            try
                            {
                                AddClassToNameSpace(objClass, eachType.Namespace, ref AsposeProductAPI);

                            }

                            catch (Exception ex)
                            {
                                WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                            }



                            var InfoPublic = eachType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(s => s.IsSpecialName == false && s.IsPublic == true).OrderBy(s => s.Name);


                            var InfoProtected = eachType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(s => s.IsVirtual == true && s.IsFinal == false);


                            if (InfoPublic != null)
                            {
                                try
                                {

                                    #region  Methods Formation
                                    List<APIClassMembers> classMembers = new List<APIClassMembers>();

                                    foreach (var eachMethod in InfoPublic)
                                    {
                                        if (!eachMethod.IsPublic)
                                            continue;

                                        APIClassMembers mem = new APIClassMembers();

                                        mem.Name = eachMethod.Name;
                                        mem.isConstructor = eachMethod.IsConstructor;
                                        mem.IsAbstract = eachMethod.IsAbstract;
                                        mem.IsFinal = eachMethod.IsFinal;
                                        mem.IsGeneric = eachMethod.IsGenericMethod;
                                        mem.IsVitual = eachMethod.IsVirtual;
                                        mem.ReturnType = eachMethod.ReturnType.ToString();

                                        ParameterInfo[] ConParams = eachMethod.GetParameters();

                                        foreach (var eachPara in ConParams)
                                        {
                                            APIClassMembersParameters par = new APIClassMembersParameters();
                                            par.ParamName = eachPara.Name;
                                            par.ParamType = eachPara.ParameterType.Name;
                                            par.IsOut = eachPara.IsOut;
                                            par.IsOptional = eachPara.IsOptional;

                                            mem.AddMethodParameter(par);

                                        }

                                        if (classMembers.SingleOrDefault(s => s.Name == mem.Name) == null)
                                            classMembers.Add(mem);
                                        else
                                        {
                                            var member = classMembers.SingleOrDefault(s => s.Name == mem.Name);
                                            member.AddAPIClassFormat(mem);

                                        }
                                    }

                                    objClass.lstMembers = classMembers;



                                    #endregion


                                }
                                catch (Exception ex)
                                {
                                    WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                                }
                            }



                            var meminfo = eachType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).OrderBy(s => s.Name);

                            if (meminfo != null)
                            {
                                try
                                {
                                    #region  Fields Formation
                                    foreach (var eachMem in meminfo)
                                    {
                                        if (!eachMem.IsPublic)
                                            continue;

                                        APIClassProperties mem = new APIClassProperties();

                                        mem.Name = eachMem.Name;
                                        mem.Type = eachMem.FieldType.Name;
                                        mem.TypeFullName = eachMem.FieldType.FullName;
                                        mem.isStatic = eachMem.IsStatic;



                                        ParsingHelper.AddClassProperty(eachType.Namespace, objClass.ClassName, mem, ref AsposeProductAPI);
                                    }
                                    #endregion
                                }

                                catch (Exception ex)
                                {
                                    WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                                }

                            }




                        }


                    }



                    #endregion
                }


                catch (Exception ex)
                {
                    WebThreadHelper.UpdateProgress(string.Format(" <br/><hr/>ERROR:{0}<br/>", ex.Message));

                }
            }




        }



        public static string[] GetEnumValues(Type enumType)
        {
            return (from fi in enumType.GetFields(BindingFlags.Public | BindingFlags.Static) select fi.Name).ToArray();
        }

        public static void FetchMemberInfo(APIClassMembers eachProp,ref ProductAPI AsposeProductAPI,XElement xmlOverload)
        {

            WebThreadHelper.UpdateProgress("Fetching Description ");
            eachProp.Description = ParsingHelper.GetDescirption(xmlOverload, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");


            WebThreadHelper.UpdateProgress("Fetching Examples Codes ");
            eachProp.lstExamples = ParsingHelper.GetExampleCodes(xmlOverload); WebThreadHelper.UpdateProgress("...Done <br/>");

            List<string> SeeAlso = new List<string>();


            WebThreadHelper.UpdateProgress("Fetching Remarks ");
            eachProp.ObjRemarks = ParsingHelper.GetRemarks(xmlOverload, ref SeeAlso, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");
            eachProp.lstSeeAlso = SeeAlso;


            WebThreadHelper.UpdateProgress("Fetching Method Return Type ");
            eachProp.Returns = ParsingHelper.GetReturns(xmlOverload, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");


            WebThreadHelper.UpdateProgress("Fetching Method Parametres ");
            eachProp.ParamComments = ParsingHelper.GetParams(xmlOverload); WebThreadHelper.UpdateProgress("...Done <br/>");
        }

        

        public static void ParseXMLCommentsFile(XDocument xdoc, ref XMLParser.Models.ProductAPI AsposeProductAPI)
        {
            var members = from item in xdoc.Descendants("member")
                          select item;


            foreach (var each in AsposeProductAPI.lstNameSpaces)
            {
                try
                {

                    try
                    {
                        var ns = members.SingleOrDefault(s => s.Attribute("name").Value == (string.Format("T:{0}", each.Title + ".NamespaceDoc"))).Descendants("summary");

                        if (ns != null)
                        {
                            if (ns.Count() > 0)
                                each.Description = ns.ElementAt(0).ToString();

                        }
                    }
                    catch (Exception ex)
                    {

                    }


                    if (each.lstClasses != null)
                        foreach (var eachClass in each.lstClasses)
                        {
                            string className = eachClass.ClassName;
                            try
                            {
                                #region Class Iteration XML

                                if (eachClass.Type == Types.ClassType.Class)
                                {

                                    #region Class Only Extraction
                                    XElement xmlClass = null;

                                    try
                                    {
                                        WebThreadHelper.UpdateProgress(string.Format("Parsing Class {0}  <br/>", className));

                                        if (!eachClass.IsDuplicatedInOtherNameSpace)
                                            xmlClass = members.SingleOrDefault(s => s.Attribute("name").Value == (string.Format("T:{0}.{1}", each.Title, eachClass.ClassName)));
                                        else
                                            xmlClass = members.SingleOrDefault(s => s.Attribute("name").Value == (string.Format("T:{0}", eachClass.ClassName)));

                                    }

                                    catch (Exception ex)
                                    {
                                        WebThreadHelper.Error("Error Fetching Comments for " + className + ", Error=" + ex.Message);
                                        WebThreadHelper.Error(" <br/>");


                                    }

                                    if (xmlClass == null)
                                        continue;

                                    try
                                    {
                                        #region Class
                                        try
                                        {

                                            eachClass.Assembly = AsposeProductAPI.AssemblyName;

                                            WebThreadHelper.UpdateProgress("Fetching Description...");
                                            eachClass.Description = ParsingHelper.GetDescirption(xmlClass, ref AsposeProductAPI);
                                            WebThreadHelper.UpdateProgress("Done<br/>");


                                            WebThreadHelper.UpdateProgress("Fetching Summary...");
                                            eachClass.SummaryObject = ParsingHelper.GetCrefObject(xmlClass);
                                            WebThreadHelper.UpdateProgress("Done<br/>");


                                            WebThreadHelper.UpdateProgress("Fetching Example Codes...");
                                            eachClass.lstExamples = ParsingHelper.GetExampleCodes(xmlClass);
                                            WebThreadHelper.UpdateProgress("Done<br/>");


                                            List<string> SeeAlso = new List<string>();

                                            WebThreadHelper.UpdateProgress("Fetching Ramarks");
                                            eachClass.ObjRemarks = ParsingHelper.GetRemarks(xmlClass, ref SeeAlso, ref AsposeProductAPI);
                                            WebThreadHelper.UpdateProgress("Done<br/>");

                                            eachClass.lstSeeAlso = SeeAlso;
                                        }
                                        catch (Exception ex)
                                        {
                                            string InnerXML = "";

                                            if (xmlClass != null)
                                                InnerXML = xmlClass.ToString();

                                            WebThreadHelper.Error("Error In XML Format- " + className + ", Inner XML ="+InnerXML);
                                            WebThreadHelper.Error(" <br/>");

                                        }

                                        #endregion
                                    }

                                    catch (Exception ex)
                                    {

                                        WebThreadHelper.Error("Error Fetching Class -" + className );
                                        WebThreadHelper.Error(" <br/>");


                                    }



                                    if (eachClass.lstProperties != null)
                                        foreach (var eachProp in eachClass.lstProperties)
                                        {
                                            try
                                            {
                                               


                                                #region Properties

                                                WebThreadHelper.UpdateProgress(string.Format("Fetching Property {0} for Class {1}", eachClass.ClassName, eachProp.Name));
                                                var xmlProperty = members.SingleOrDefault(s => s.Attribute("name").Value == (string.Format("P:{0}.{1}.{2}", each.Title, eachClass.ClassName, eachProp.Alias)));

                                                if (xmlProperty == null)
                                                {


                                                    var Format = members.SingleOrDefault(s => s.Attribute("name").Value == (string.Format("F:{0}.{1}.{2}", each.Title, eachClass.ClassName, eachProp.Name)));

                                                    if (Format != null)
                                                    {
                                                        WebThreadHelper.UpdateProgress("Fetching Description ");

                                                        eachProp.Description = ParsingHelper.GetDescirption(Format, ref AsposeProductAPI);

                                                        WebThreadHelper.UpdateProgress("Done<br/>");

                                                        WebThreadHelper.UpdateProgress("Fetching Example Code ");

                                                        eachProp.lstExamples = ParsingHelper.GetExampleCodes(Format);

                                                        WebThreadHelper.UpdateProgress("Done<br/>");

                                                        List<string> SeeAlso = new List<string>();

                                                        WebThreadHelper.UpdateProgress("Fetching Remarks ");

                                                        eachProp.ObjRemarks = ParsingHelper.GetRemarks(Format, ref SeeAlso, ref AsposeProductAPI);

                                                        WebThreadHelper.UpdateProgress("Done<br/>");
                                                        eachProp.lstSeeAlso = SeeAlso;

                                                    }
                                                }
                                                else
                                                {

                                                    try
                                                    {
                                                        WebThreadHelper.UpdateProgress("Fetching Description ");

                                                        eachProp.Description = ParsingHelper.GetDescirption(xmlProperty, ref AsposeProductAPI);

                                                        WebThreadHelper.UpdateProgress("Done<br/>");

                                                        WebThreadHelper.UpdateProgress("Fetching Example Code ");

                                                        eachProp.lstExamples = ParsingHelper.GetExampleCodes(xmlProperty);

                                                        WebThreadHelper.UpdateProgress("Done<br/>");

                                                        List<string> SeeAlso = new List<string>();

                                                        WebThreadHelper.UpdateProgress("Fetching Remarks ");

                                                        eachProp.ObjRemarks = ParsingHelper.GetRemarks(xmlProperty, ref SeeAlso, ref AsposeProductAPI);

                                                        WebThreadHelper.UpdateProgress("Done<br/>");
                                                        eachProp.lstSeeAlso = SeeAlso;

                                                        WebThreadHelper.UpdateProgress("Done<br/>");
                                                    }

                                                    catch (Exception ex)
                                                    {
                                                        WebThreadHelper.Error(string.Format("Error In Source XML -{0}.{1} ", eachProp.Name, eachClass.ClassName));
                                                        WebThreadHelper.Error(" <br/>");

                                                    }



                                                }
                                                #endregion

                                            }

                                            catch (Exception ex)
                                            {
                                                WebThreadHelper.UpdateProgress("<br/>");
                                                WebThreadHelper.Error(string.Format("Error In Property {0} in Class  {1} ", eachProp.Name, eachClass.ClassName) + ", Error=" + ex.Message);
                                                WebThreadHelper.Error(" <br/>");


                                            }

                                        }

                                    if (eachClass.lstMembers != null)
                                        foreach (var eachProp in eachClass.lstMembers)
                                        {
                                            try
                                            {
                                                #region Constructor

                                                if (eachProp.isConstructor)
                                                {


                                                    eachProp.Alias = className + " Constructor";
                                                    eachProp.Name = className + " Constructor";



                                                    if (eachProp.lstOverLoads != null && eachProp.lstOverLoads.Count > 0)
                                                        foreach (var eachOv in eachProp.lstOverLoads)
                                                        {

                                                            string param = "";

                                                            foreach (var eachP in eachOv.lstParams)
                                                            {
                                                                if (param != "")
                                                                    param += ",";

                                                                param += eachP.ParamFullName;

                                                            }

                                                            var methodOverloadMain = members.SingleOrDefault
                                                   (s => s.Attribute("name").Value == ((string.Format("M:{0}.{1}.#ctor({2})", each.Title, eachClass.ClassName, param))));


                                                            FetchMemberInfo(eachOv, ref AsposeProductAPI, methodOverloadMain);


                                                        }


                                                    var methodOverload = members.SingleOrDefault
                                                 (s => s.Attribute("name").Value == ((string.Format("M:{0}.{1}.{2}", each.Title, eachClass.ClassName, "#ctor"))));

                                                    if (methodOverload != null)
                                                        FetchMemberInfo(eachProp, ref AsposeProductAPI, methodOverload);



                                                    continue;
                                                }
                                                #endregion

                                                if (eachProp.lstOverLoads == null || eachProp.lstOverLoads.Count == 0)
                                                {
                                                    if (!eachClass.IsDuplicatedInOtherNameSpace)
                                                        ExtractXML(eachProp, members, ref AsposeProductAPI, each.Title, eachClass.ClassName);
                                                    else
                                                        ExtractXML(eachProp, members, ref AsposeProductAPI, each.Title, eachClass.ClassName, true);
                                                }
                                                else
                                                {
                                                    if (!eachClass.IsDuplicatedInOtherNameSpace)
                                                        ExtractXML(eachProp, members, ref AsposeProductAPI, each.Title, eachClass.ClassName);
                                                    else
                                                        ExtractXML(eachProp, members, ref AsposeProductAPI, each.Title, eachClass.ClassName, true);

                                                    if (eachProp.lstOverLoads != null && eachProp.lstOverLoads.Count > 0)
                                                        foreach (var eachOv in eachProp.lstOverLoads)
                                                        {

                                                            if (!eachClass.IsDuplicatedInOtherNameSpace)
                                                                ExtractXML(eachOv, members, ref AsposeProductAPI, each.Title, eachClass.ClassName);
                                                            else ExtractXML(eachOv, members, ref AsposeProductAPI, each.Title, eachClass.ClassName, true);

                                                        }


                                                }


                                                if (eachProp.IsDuplicated)
                                                {
                                                    eachProp.Alias = eachProp.Name;
                                                    eachProp.Name = eachClass.NameSpace + "." + eachClass.ClassName + "." + eachProp.Name;

                                                    if (eachProp.lstOverLoads != null)
                                                    {
                                                        foreach (var eachO in eachProp.lstOverLoads)
                                                        {
                                                            eachO.Name = eachClass.NameSpace + "." + eachClass.ClassName + "." + eachO.Name;
                                                        }
                                                    }
                                                }



                                            }


                                            catch (Exception ex)
                                            {


                                                WebThreadHelper.Error(string.Format("Error In Method {0} in Class  {1} ", eachProp.Name, eachClass.ClassName) + ", Error=" + ex.Message);
                                                WebThreadHelper.Error(" <br/>");

                                            }

                                        }
                                    #endregion
                                }

                                else if (eachClass.Type == Types.ClassType.Enumeration)
                                {
                                    #region Enumeration Extraction
                                    XElement xmlClass = null;

                                    try
                                    {
                                        WebThreadHelper.UpdateProgress(string.Format("Parsing Enumeration {0}  <br/>", className));

                                        xmlClass = members.SingleOrDefault(s => s.Attribute("name").Value == (string.Format("T:{0}.{1}", each.Title, eachClass.ClassName)));
                                    }

                                    catch (Exception ex)
                                    {
                                        WebThreadHelper.Error("Error Fetching Comments for " + className + ", Error=" + ex.Message);
                                        WebThreadHelper.Error(" <br/>");


                                    }

                                    if (xmlClass == null)
                                        continue;

                                    WebThreadHelper.UpdateProgress("Fetching Description...");
                                    eachClass.Description = ParsingHelper.GetDescirption(xmlClass, ref AsposeProductAPI);
                                    WebThreadHelper.UpdateProgress("Done<br/>");

                                    eachClass.Assembly = AsposeProductAPI.AssemblyName;

                                    if (eachClass.lstEnumTypes != null)
                                        if (eachClass.lstEnumTypes.Count > 0)
                                            foreach (var eachItem in eachClass.lstEnumTypes)
                                            {

                                                var elem = members.SingleOrDefault(s => s.Attribute("name").Value == (string.Format("F:{0}.{1}.{2}", each.Title, eachClass.ClassName, eachItem.Name)));

                                                if (elem == null) continue;
                                                eachItem.Description = ParsingHelper.GetDescirption(elem, ref AsposeProductAPI);


                                            }


                                    List<string> SeeAlso = new List<string>();

                                    WebThreadHelper.UpdateProgress("Fetching Ramarks");
                                    eachClass.ObjRemarks = ParsingHelper.GetRemarks(xmlClass, ref SeeAlso, ref AsposeProductAPI);
                                    WebThreadHelper.UpdateProgress("Done<br/>");

                                    eachClass.lstSeeAlso = SeeAlso;


                                    #endregion
                                }


                                else if (eachClass.Type == Types.ClassType.Structure)
                                {

                                    #region Structure  Extraction
                                    XElement xmlClass = null;

                                    try
                                    {
                                        WebThreadHelper.UpdateProgress(string.Format("Parsing Class {0}  <br/>", className));

                                        xmlClass = members.SingleOrDefault(s => s.Attribute("name").Value == (string.Format("T:{0}.{1}", each.Title, eachClass.ClassName)));
                                    }

                                    catch (Exception ex)
                                    {
                                        WebThreadHelper.Error("Error Fetching Comments for " + className + ", Error=" + ex.Message);
                                        WebThreadHelper.Error(" <br/>");


                                    }

                                    if (xmlClass == null)
                                        continue;

                                    try
                                    {
                                        #region Class

                                        WebThreadHelper.UpdateProgress("Fetching Description...");
                                        eachClass.Description = ParsingHelper.GetDescirption(xmlClass, ref AsposeProductAPI);
                                        WebThreadHelper.UpdateProgress("Done<br/>");

                                        eachClass.Assembly = AsposeProductAPI.AssemblyName;

                                        WebThreadHelper.UpdateProgress("Fetching Summary...");
                                        eachClass.SummaryObject = ParsingHelper.GetCrefObject(xmlClass);
                                        WebThreadHelper.UpdateProgress("Done<br/>");


                                        WebThreadHelper.UpdateProgress("Fetching Example Codes...");
                                        eachClass.lstExamples = ParsingHelper.GetExampleCodes(xmlClass);
                                        WebThreadHelper.UpdateProgress("Done<br/>");


                                        List<string> SeeAlso = new List<string>();

                                        WebThreadHelper.UpdateProgress("Fetching Ramarks");
                                        eachClass.ObjRemarks = ParsingHelper.GetRemarks(xmlClass, ref SeeAlso, ref AsposeProductAPI);
                                        WebThreadHelper.UpdateProgress("Done<br/>");

                                        eachClass.lstSeeAlso = SeeAlso;

                                        #endregion
                                    }

                                    catch (Exception ex)
                                    {

                                        WebThreadHelper.Error("Error Fetching Class" + className + ", Error=" + ex.Message);
                                        WebThreadHelper.Error(" <br/>");


                                    }



                                    if (eachClass.lstProperties != null)
                                        foreach (var eachProp in eachClass.lstProperties)
                                        {
                                            try
                                            { 

                                         

                                                #region Properties

                                                WebThreadHelper.UpdateProgress(string.Format("Fetching Property {0} for Class {1}", eachClass.ClassName, eachProp.Name));

                                                var Format = members.SingleOrDefault(s => s.Attribute("name").Value == (string.Format("F:{0}.{1}.{2}", each.Title, eachClass.ClassName, eachProp.Name)));

                                                if (Format != null)
                                                {
                                                    WebThreadHelper.UpdateProgress("Fetching Description ");

                                                    eachProp.Description = ParsingHelper.GetDescirption(Format, ref AsposeProductAPI);

                                                    WebThreadHelper.UpdateProgress("Done<br/>");

                                                    WebThreadHelper.UpdateProgress("Fetching Example Code ");

                                                    eachProp.lstExamples = ParsingHelper.GetExampleCodes(Format);

                                                    WebThreadHelper.UpdateProgress("Done<br/>");

                                                    List<string> SeeAlso = new List<string>();

                                                    WebThreadHelper.UpdateProgress("Fetching Remarks ");

                                                    eachProp.ObjRemarks = ParsingHelper.GetRemarks(Format, ref SeeAlso, ref AsposeProductAPI);

                                                    WebThreadHelper.UpdateProgress("Done<br/>");
                                                    eachProp.lstSeeAlso = SeeAlso;

                                                }


                                                #endregion

                                            }

                                            catch (Exception ex)
                                            {
                                                WebThreadHelper.UpdateProgress("<br/>");
                                                WebThreadHelper.Error(string.Format("Error In Source XML -  {0} in Class  {1} ", eachProp.Name, eachClass.ClassName) );

                                                WebThreadHelper.Error(" <br/>");


                                            }

                                        }

                                    if (eachClass.lstMembers != null)
                                        foreach (var eachProp in eachClass.lstMembers)
                                        {
                                            try
                                            {



                                                if (eachProp.lstOverLoads == null || eachProp.lstOverLoads.Count == 0)
                                                {
                                                    #region Case -I - No Over Load

                                                    WebThreadHelper.UpdateProgress(string.Format("Fetching Method {0} for Class {1}- With No Overload", eachClass.ClassName, eachProp.Name));

                                                    var xmlProperty = members.SingleOrDefault(s => s.Attribute("name").Value.Contains(string.Format("M:{0}.{1}.{2}", each.Title, eachClass.ClassName, eachProp.Name)));
                                                    WebThreadHelper.UpdateProgress("...Done <br/>");
                                                    if (xmlProperty != null)
                                                    {
                                                        #region Methods Member Extraction

                                                        WebThreadHelper.UpdateProgress("Fetching Description ");
                                                        eachProp.Description = ParsingHelper.GetDescirption(xmlProperty, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                        WebThreadHelper.UpdateProgress("Fetching Example Codes ");
                                                        eachProp.lstExamples = ParsingHelper.GetExampleCodes(xmlProperty); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                        List<string> SeeAlso = new List<string>();

                                                        WebThreadHelper.UpdateProgress("Fetching Remarks ");
                                                        eachProp.ObjRemarks = ParsingHelper.GetRemarks(xmlProperty, ref SeeAlso, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");
                                                        eachProp.lstSeeAlso = SeeAlso;

                                                        WebThreadHelper.UpdateProgress("Fetching Method Return Type ");
                                                        eachProp.Returns = ParsingHelper.GetReturns(xmlProperty, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                        WebThreadHelper.UpdateProgress("Fetching Method Parameteres ");

                                                        eachProp.ParamComments = ParsingHelper.GetParams(xmlProperty); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                        #endregion
                                                    }

                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region Case -II With OverLoads

                                                    #region Overloads
                                                    WebThreadHelper.UpdateProgress(string.Format("Fetching Method {0} for Class {1}- With  Overload", eachClass.ClassName, eachProp.Name));

                                                    var methodOverload = members.Where
                                                        (s => s.Attribute("name").Value.Contains((string.Format("M:{0}.{1}.{2}", each.Title, eachClass.ClassName, eachProp.Name))));
                                                    WebThreadHelper.UpdateProgress("...Done <br/>");

                                                    foreach (var xmlOverload in methodOverload)
                                                    {

                                                        string mname = xmlOverload.Attribute("name").Value.ToString();

                                                        bool isMatch = false;

                                                        int ParamLength = 0;


                                                        string[] array = mname.Split('(');

                                                        if (array.Length >= 2)
                                                        {

                                                            if (eachProp.lstParams == null)
                                                                continue;

                                                            var paramList = array[1].Replace(")", "");

                                                            string[] paramCOunt = paramList.Split(',');

                                                            ParamLength = paramCOunt.Length;

                                                            if (eachProp.lstParams.Count == paramCOunt.Length)
                                                                isMatch = true;
                                                        }
                                                        else
                                                        {
                                                            if (eachProp.lstParams == null)
                                                                isMatch = true;
                                                            else
                                                            {
                                                                if (eachProp.lstParams.Count == 0)
                                                                    isMatch = true;

                                                            }

                                                        }




                                                        if (isMatch)
                                                        {
                                                            // Fetch XML 
                                                            #region Methods Member Extraction

                                                            WebThreadHelper.UpdateProgress("Fetching Description ");
                                                            eachProp.Description = ParsingHelper.GetDescirption(xmlOverload, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");


                                                            WebThreadHelper.UpdateProgress("Fetching Examples Codes ");
                                                            eachProp.lstExamples = ParsingHelper.GetExampleCodes(xmlOverload); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                            List<string> SeeAlso = new List<string>();


                                                            WebThreadHelper.UpdateProgress("Fetching Remarks ");
                                                            eachProp.ObjRemarks = ParsingHelper.GetRemarks(xmlOverload, ref SeeAlso, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");
                                                            eachProp.lstSeeAlso = SeeAlso;


                                                            WebThreadHelper.UpdateProgress("Fetching Method Return Type ");
                                                            eachProp.Returns = ParsingHelper.GetReturns(xmlOverload, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");


                                                            WebThreadHelper.UpdateProgress("Fetching Method Parametres ");
                                                            eachProp.ParamComments = ParsingHelper.GetParams(xmlOverload); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                            #endregion

                                                        }

                                                        else
                                                        {

                                                            foreach (var eachOverLoad in eachProp.lstOverLoads)
                                                            {

                                                                if (eachOverLoad.lstParams.Count == ParamLength)
                                                                {

                                                                    //Fetch XML 
                                                                    #region Methods Member Extraction

                                                                    WebThreadHelper.UpdateProgress("Fetching Description ");
                                                                    eachOverLoad.Description = ParsingHelper.GetDescirption(xmlOverload, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                                    WebThreadHelper.UpdateProgress("Fetching Examples Codes ");
                                                                    eachOverLoad.lstExamples = ParsingHelper.GetExampleCodes(xmlOverload); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                                    List<string> SeeAlso = new List<string>();

                                                                    WebThreadHelper.UpdateProgress("Fetching Remarks ");
                                                                    eachOverLoad.ObjRemarks = ParsingHelper.GetRemarks(xmlOverload, ref SeeAlso, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");
                                                                    eachOverLoad.lstSeeAlso = SeeAlso;


                                                                    WebThreadHelper.UpdateProgress("Fetching Method Return Type ");
                                                                    eachOverLoad.Returns = ParsingHelper.GetReturns(xmlOverload, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");


                                                                    WebThreadHelper.UpdateProgress("Fetching Method Parametres ");
                                                                    eachOverLoad.ParamComments = ParsingHelper.GetParams(xmlOverload); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                                    #endregion
                                                                }

                                                            }


                                                        }

                                                    }

                                                    #endregion

                                                    #endregion

                                                }







                                            }

                                            catch (Exception ex)
                                            {


                                                WebThreadHelper.Error(string.Format("Error In Method {0} in Class  {1} ", eachProp.Name, eachClass.ClassName) + ", Error=" + ex.Message);
                                                WebThreadHelper.Error(" <br/>");

                                            }

                                        }





                                    #endregion
                                }

                                else if (eachClass.Type == Types.ClassType.Interface)
                                {

                                    #region Interface  Extraction
                                    XElement xmlClass = null;

                                    try
                                    {
                                        WebThreadHelper.UpdateProgress(string.Format("Parsing Class {0}  <br/>", className));

                                        xmlClass = members.SingleOrDefault(s => s.Attribute("name").Value == (string.Format("T:{0}.{1}", each.Title, eachClass.ClassName)));
                                    }

                                    catch (Exception ex)
                                    {
                                        WebThreadHelper.Error("Error Fetching Comments for " + className + ", Error=" + ex.Message);
                                        WebThreadHelper.Error(" <br/>");


                                    }

                                    if (xmlClass == null)
                                        continue;

                                    try
                                    {
                                        #region Class

                                        WebThreadHelper.UpdateProgress("Fetching Description...");
                                        eachClass.Description = ParsingHelper.GetDescirption(xmlClass, ref AsposeProductAPI);
                                        WebThreadHelper.UpdateProgress("Done<br/>");

                                        eachClass.Assembly = AsposeProductAPI.AssemblyName;

                                        WebThreadHelper.UpdateProgress("Fetching Summary...");
                                        eachClass.SummaryObject = ParsingHelper.GetCrefObject(xmlClass);
                                        WebThreadHelper.UpdateProgress("Done<br/>");


                                        WebThreadHelper.UpdateProgress("Fetching Example Codes...");
                                        eachClass.lstExamples = ParsingHelper.GetExampleCodes(xmlClass);
                                        WebThreadHelper.UpdateProgress("Done<br/>");


                                        List<string> SeeAlso = new List<string>();

                                        WebThreadHelper.UpdateProgress("Fetching Ramarks");
                                        eachClass.ObjRemarks = ParsingHelper.GetRemarks(xmlClass, ref SeeAlso, ref AsposeProductAPI);
                                        WebThreadHelper.UpdateProgress("Done<br/>");

                                        eachClass.lstSeeAlso = SeeAlso;

                                        #endregion
                                    }

                                    catch (Exception ex)
                                    {

                                        WebThreadHelper.Error("Error Fetching Class" + className + ", Error=" + ex.Message);
                                        WebThreadHelper.Error(" <br/>");


                                    }



                                    if (eachClass.lstProperties != null)
                                        foreach (var eachProp in eachClass.lstProperties)
                                        {
                                            try
                                            {
                                                if (eachProp.Name == "FilterColumnCollection")
                                                {


                                                }


                                                #region Properties

                                                WebThreadHelper.UpdateProgress(string.Format("Fetching Property {0} for Class {1}", eachClass.ClassName, eachProp.Name));

                                                var Format = members.SingleOrDefault(s => s.Attribute("name").Value == (string.Format("P:{0}.{1}.{2}", each.Title, eachClass.ClassName, eachProp.Name)));

                                                if (Format != null)
                                                {
                                                    WebThreadHelper.UpdateProgress("Fetching Description ");

                                                    eachProp.Description = ParsingHelper.GetDescirption(Format, ref AsposeProductAPI);

                                                    WebThreadHelper.UpdateProgress("Done<br/>");

                                                    WebThreadHelper.UpdateProgress("Fetching Example Code ");

                                                    eachProp.lstExamples = ParsingHelper.GetExampleCodes(Format);

                                                    WebThreadHelper.UpdateProgress("Done<br/>");

                                                    List<string> SeeAlso = new List<string>();

                                                    WebThreadHelper.UpdateProgress("Fetching Remarks ");

                                                    eachProp.ObjRemarks = ParsingHelper.GetRemarks(Format, ref SeeAlso, ref AsposeProductAPI);

                                                    WebThreadHelper.UpdateProgress("Done<br/>");
                                                    eachProp.lstSeeAlso = SeeAlso;

                                                }


                                                #endregion

                                            }

                                            catch (Exception ex)
                                            {
                                                WebThreadHelper.UpdateProgress("<br/>");
                                                WebThreadHelper.Error(string.Format("Error In Property {0} in Class  {1} ", eachProp.Name, eachClass.ClassName) + ", Error=" + ex.Message);
                                                WebThreadHelper.Error(" <br/>");


                                            }

                                        }

                                    if (eachClass.lstMembers != null)
                                        foreach (var eachProp in eachClass.lstMembers)
                                        {
                                            try
                                            {



                                                if (eachProp.lstOverLoads == null || eachProp.lstOverLoads.Count == 0)
                                                {
                                                    #region Case -I - No Over Load

                                                    WebThreadHelper.UpdateProgress(string.Format("Fetching Method {0} for Class {1}- With No Overload", eachClass.ClassName, eachProp.Name));

                                                    var xmlProperty = members.SingleOrDefault(s => s.Attribute("name").Value.Contains(string.Format("M:{0}.{1}.{2}", each.Title, eachClass.ClassName, eachProp.Name)));
                                                    WebThreadHelper.UpdateProgress("...Done <br/>");
                                                    if (xmlProperty != null)
                                                    {
                                                        #region Methods Member Extraction

                                                        WebThreadHelper.UpdateProgress("Fetching Description ");
                                                        eachProp.Description = ParsingHelper.GetDescirption(xmlProperty, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                        WebThreadHelper.UpdateProgress("Fetching Example Codes ");
                                                        eachProp.lstExamples = ParsingHelper.GetExampleCodes(xmlProperty); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                        List<string> SeeAlso = new List<string>();

                                                        WebThreadHelper.UpdateProgress("Fetching Remarks ");
                                                        eachProp.ObjRemarks = ParsingHelper.GetRemarks(xmlProperty, ref SeeAlso, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");
                                                        eachProp.lstSeeAlso = SeeAlso;

                                                        WebThreadHelper.UpdateProgress("Fetching Method Return Type ");
                                                        eachProp.Returns = ParsingHelper.GetReturns(xmlProperty, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                        WebThreadHelper.UpdateProgress("Fetching Method Parameteres ");

                                                        eachProp.ParamComments = ParsingHelper.GetParams(xmlProperty); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                        #endregion
                                                    }

                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region Case -II With OverLoads

                                                    #region Overloads
                                                    WebThreadHelper.UpdateProgress(string.Format("Fetching Method {0} for Class {1}- With  Overload", eachClass.ClassName, eachProp.Name));

                                                    var methodOverload = members.Where
                                                        (s => s.Attribute("name").Value.Contains((string.Format("M:{0}.{1}.{2}", each.Title, eachClass.ClassName, eachProp.Name))));
                                                    WebThreadHelper.UpdateProgress("...Done <br/>");

                                                    foreach (var xmlOverload in methodOverload)
                                                    {

                                                        string mname = xmlOverload.Attribute("name").Value.ToString();

                                                        bool isMatch = false;

                                                        int ParamLength = 0;


                                                        string[] array = mname.Split('(');

                                                        if (array.Length >= 2)
                                                        {

                                                            if (eachProp.lstParams == null)
                                                                continue;

                                                            var paramList = array[1].Replace(")", "");

                                                            string[] paramCOunt = paramList.Split(',');

                                                            ParamLength = paramCOunt.Length;

                                                            if (eachProp.lstParams.Count == paramCOunt.Length)
                                                                isMatch = true;
                                                        }
                                                        else
                                                        {
                                                            if (eachProp.lstParams == null)
                                                                isMatch = true;
                                                            else
                                                            {
                                                                if (eachProp.lstParams.Count == 0)
                                                                    isMatch = true;

                                                            }

                                                        }




                                                        if (isMatch)
                                                        {
                                                            // Fetch XML 
                                                            #region Methods Member Extraction

                                                            WebThreadHelper.UpdateProgress("Fetching Description ");
                                                            eachProp.Description = ParsingHelper.GetDescirption(xmlOverload, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");


                                                            WebThreadHelper.UpdateProgress("Fetching Examples Codes ");
                                                            eachProp.lstExamples = ParsingHelper.GetExampleCodes(xmlOverload); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                            List<string> SeeAlso = new List<string>();


                                                            WebThreadHelper.UpdateProgress("Fetching Remarks ");
                                                            eachProp.ObjRemarks = ParsingHelper.GetRemarks(xmlOverload, ref SeeAlso, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");
                                                            eachProp.lstSeeAlso = SeeAlso;


                                                            WebThreadHelper.UpdateProgress("Fetching Method Return Type ");
                                                            eachProp.Returns = ParsingHelper.GetReturns(xmlOverload, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");


                                                            WebThreadHelper.UpdateProgress("Fetching Method Parametres ");
                                                            eachProp.ParamComments = ParsingHelper.GetParams(xmlOverload); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                            #endregion

                                                        }

                                                        else
                                                        {

                                                            foreach (var eachOverLoad in eachProp.lstOverLoads)
                                                            {

                                                                if (eachOverLoad.lstParams.Count == ParamLength)
                                                                {

                                                                    //Fetch XML 
                                                                    #region Methods Member Extraction

                                                                    WebThreadHelper.UpdateProgress("Fetching Description ");
                                                                    eachOverLoad.Description = ParsingHelper.GetDescirption(xmlOverload, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                                    WebThreadHelper.UpdateProgress("Fetching Examples Codes ");
                                                                    eachOverLoad.lstExamples = ParsingHelper.GetExampleCodes(xmlOverload); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                                    List<string> SeeAlso = new List<string>();

                                                                    WebThreadHelper.UpdateProgress("Fetching Remarks ");
                                                                    eachOverLoad.ObjRemarks = ParsingHelper.GetRemarks(xmlOverload, ref SeeAlso, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");
                                                                    eachOverLoad.lstSeeAlso = SeeAlso;


                                                                    WebThreadHelper.UpdateProgress("Fetching Method Return Type ");
                                                                    eachOverLoad.Returns = ParsingHelper.GetReturns(xmlOverload, ref AsposeProductAPI); WebThreadHelper.UpdateProgress("...Done <br/>");


                                                                    WebThreadHelper.UpdateProgress("Fetching Method Parametres ");
                                                                    eachOverLoad.ParamComments = ParsingHelper.GetParams(xmlOverload); WebThreadHelper.UpdateProgress("...Done <br/>");

                                                                    #endregion
                                                                }

                                                            }


                                                        }

                                                    }

                                                    #endregion

                                                    #endregion

                                                }







                                            }

                                            catch (Exception ex)
                                            {


                                                WebThreadHelper.Error(string.Format("Error In Method {0} in Class  {1} ", eachProp.Name, eachClass.ClassName) + ", Error=" + ex.Message);
                                                WebThreadHelper.Error(" <br/>");

                                            }

                                        }





                                    #endregion
                                }

                                else if (eachClass.Type == Types.ClassType.Delegate)
                                {

                                    #region Delegate  Extraction
                                    XElement xmlClass = null;

                                    try
                                    {
                                        WebThreadHelper.UpdateProgress(string.Format("Parsing Delegate {0}  <br/>", className));

                                        xmlClass = members.SingleOrDefault(s => s.Attribute("name").Value == (string.Format("T:{0}.{1}", each.Title, eachClass.ClassName)));

                                        WebThreadHelper.UpdateProgress("Fetching Description...");
                                        eachClass.Description = ParsingHelper.GetDescirption(xmlClass, ref AsposeProductAPI);
                                        WebThreadHelper.UpdateProgress("Done<br/>");


                                        var parms = xmlClass.Elements();

                                        foreach (var eP in parms)
                                        {
                                            if (eP.Name == "param")
                                            {
                                                eachClass.DelegateInfo.Set(eP.Attribute("name").ToString(), eP.Value.ToString());
                                            }

                                        }

                                    }

                                    catch (Exception ex)
                                    {
                                        WebThreadHelper.Error("Error Fetching Comments for " + className + ", Error=" + ex.Message);
                                        WebThreadHelper.Error(" <br/>");


                                    }

                                    #endregion


                                }
                                #endregion

                            }
                            catch (Exception ex)
                            {

                                WebThreadHelper.Error(" Error=" + ex.Message);
                            }
                        }
                }

                catch (Exception ex)
                {
                    WebThreadHelper.Error(" Error=" + ex.Message);
                    WebThreadHelper.Error(" <br/>");
                    
                }

            }


           

            //    if (TypeName.Contains("T:"))
            //    {


            //        TypeName = TypeName.Replace("T:", "");
            //        string className = string.Empty;
            //        string nameSpace = string.Empty;
            //        ParsingHelper.ExtractClass(TypeName, ref className, ref nameSpace);

            //        APINameSpace nSOld = AsposeProductAPI.lstNameSpaces.SingleOrDefault(s => s.Title == nameSpace);

            //        if (nSOld == null) 
            //            continue;

            //        APIClass objAPIClass = nSOld.lstClasses.SingleOrDefault(s => s.ClassName == className);

            //        if (objAPIClass == null)
            //            continue;

            //        objAPIClass.Description = Descrtiption;
            //        objAPIClass.Assembly = AsposeProductAPI.AssemblyName;






            //        try
            //        {
            //            #region Sumamry Extraction
            //            if (each.Element("summary") != null)
            //                if (each.Element("summary").Descendants().Count() >= 1)
            //                {

            //                    string Summary = each.Element("summary").Value;


            //                    foreach (var eachNodes in each.Element("summary").DescendantsAndSelf())
            //                    {
            //                        if (eachNodes.Attribute("cref") != null)
            //                        {

            //                            string summaryClassName = string.Empty;
            //                            string summaryNameSpace = string.Empty;

            //                            ParsingHelper.ExtractClass(eachNodes.Attribute("cref").Value, ref summaryClassName, ref summaryNameSpace);

            //                            if (string.IsNullOrEmpty(objAPIClass.SummaryObject))
            //                                objAPIClass.SummaryObject = summaryClassName;
            //                            else
            //                                objAPIClass.SummaryObject += ";" + summaryClassName;



            //                        }

            //                        if (eachNodes.Name == "Summary")
            //                        {

            //                        }


            //                    }


            //                }
            //            #endregion
            //        }
            //        catch (Exception ex)
            //        {
            //            //Errror in Summary
            //        }





            //      

            //        objAPIClass.Remarks = Remark;
            //        objAPIClass.ExampleCode = ExampleCode;
            //        objAPIClass.ExampleText = ExampleText;

            //        AddClassToNameSpace(objAPIClass, nameSpace, ref AsposeProductAPI);


            //        switch (objAPIClass.Type)
            //        {

            //            case Types.ClassType.Class: WebThreadHelper.UpdateProgress(string.Format("Found Class -{0} in Namespace {1}  <br/>", objAPIClass.ClassName, objAPIClass.NameSpace));
            //                break;

            //            case Types.ClassType.Interface: WebThreadHelper.UpdateProgress(string.Format("Found Interface -{0} in Namespace {1}  <br/>", objAPIClass.ClassName, objAPIClass.NameSpace));
            //                break;


            //            case Types.ClassType.Structure: WebThreadHelper.UpdateProgress(string.Format("Found Structure -{0} in Namespace {1}  <br/>", objAPIClass.ClassName, objAPIClass.NameSpace));
            //                break;


            //            case Types.ClassType.Enumeration: WebThreadHelper.UpdateProgress(string.Format("Found Enumeration -{0} in Namespace {1}  <br/>", objAPIClass.ClassName, objAPIClass.NameSpace));
            //                break;

            //        }


            //    }

            //    if (TypeName.Contains("P:"))
            //    {
            //        try
            //        {
            //            #region Property Extractor
            //            string nameSp = string.Empty;
            //            string className = string.Empty;
            //            string propertyName = string.Empty;
            //            TypeName = TypeName.Replace("P:", "");
            //            ParsingHelper.ExtractProperties(ref nameSp, TypeName, ref className, ref propertyName);

            //            APINameSpace nSOld = AsposeProductAPI.lstNameSpaces.SingleOrDefault(s => s.Title == nameSp);

            //        if (nSOld == null) 
            //            continue;

            //        APIClass objAPIClass = nSOld.lstClasses.SingleOrDefault(s => s.ClassName == className);

            //        if (objAPIClass == null)
            //            continue;


            //        APIClassProperties properties = objAPIClass.lstProperties.SingleOrDefault(s => s.Name == propertyName);

            //        if (properties == null)
            //            continue;


            //            if (each.Element("example") != null)
            //                ExampleText = each.Element("example").Value;

            //            if (each.Element("code") != null)
            //                ExampleCode = each.Element("code").Value;




            //            properties.Name = propertyName;
            //            properties.Description = Descrtiption;

            //            WebThreadHelper.UpdateProgress(string.Format("Found Class Property -{0} in  {1} Class <br/>", className, propertyName));

            //            ParsingHelper.AddClassProperty(nameSp, className, properties, ref AsposeProductAPI);
            //            #endregion

            //        }

            //        catch (Exception ex)
            //        {
            //            //Log Ex Continue Extracting Next Node;
            //        }
            //    }

            //    if (TypeName.Contains("E:"))
            //    {
            //        try
            //        {

            //            #region Method Extractor
            //            string nameSpace = string.Empty;
            //            string className = string.Empty;
            //            string methodName = string.Empty;
            //            string MethodSignature = string.Empty;


            //            ParsingHelper.ExtractEvent(TypeName, ref nameSpace, ref className, ref methodName, ref MethodSignature);




            //            WebThreadHelper.UpdateProgress(string.Format("Extracting Event({0}) <br/>", methodName));

            //            APIClassMembers APIMem = new APIClassMembers();

            //            APIMem.IsEvent = true;

            //            TypeName = TypeName.Replace("E:", "");


            //            APIMem.isConstructor = false;
            //            APIMem.Name = methodName;



            //            APIMem.Signature = MethodSignature;


            //            if (each.Element("example") != null)
            //            {
            //                APIMem.Example = each.Element("example").Value;

            //                var nodes = from s in each.Element("example").DescendantsAndSelf()
            //                            select new
            //                            {
            //                                data = s.Value,
            //                                code = s.Element("code") == null ? string.Empty : s.Element("code").Value
            //                            };

            //                string ExampleTextMethod = string.Empty;
            //                string CodeTextMethod = string.Empty;

            //                foreach (var eachExamples in nodes)
            //                {
            //                    ExampleTextMethod = eachExamples.data;
            //                    ExampleCode = eachExamples.code;
            //                    break;


            //                }


            //                ExampleTextMethod = ExampleTextMethod.Replace(ExampleCode, "");

            //                APIMem.Example = ExampleTextMethod;
            //                APIMem.CodeText = ExampleCode;
            //            }






            //            if (each.Element("summary") != null)
            //            {

            //                WebThreadHelper.UpdateProgress(string.Format("Extracting Method Summary of {0} <br/>", methodName));
            //                #region summary extractor
            //                var nodes = from s in each.Element("summary").Descendants()
            //                            select new
            //                            {
            //                                Href = s.Attribute("cref") == null ? string.Empty : s.Attribute("cref").Value,
            //                                data = s.Element("summary") == null ? string.Empty : s.Element("summary").Value
            //                            };


            //                string ClassType = string.Empty;

            //                if (nodes != null)
            //                {
            //                    if (nodes.Count() > 0)
            //                        ClassType = nodes.ElementAt(0).Href.Replace("T:", "");

            //                }

            //                APIMem.InitObject = ClassType;

            //                string SummaryText = string.Empty;

            //                foreach (var eachRetNode in each.Element("summary").Nodes())
            //                {

            //                    if (eachRetNode.ToString().Contains("<"))
            //                        SummaryText += "{object}";
            //                    else
            //                        SummaryText += eachRetNode.ToString();

            //                }


            //                APIMem.Description = SummaryText;
            //                #endregion
            //            }


            //            if (each.Element("returns") != null)
            //            {
            //                if (methodName.Contains("EnumerateMapiMessages"))
            //                {

            //                }

            //                WebThreadHelper.UpdateProgress(string.Format("Extracting Return Values of {0} <br/>", methodName));
            //                #region Returns
            //                var nodes = from s in each.Element("returns").Descendants()
            //                            select new
            //                            {
            //                                Href = s.Attribute("cref") == null ? string.Empty : s.Attribute("cref").Value,
            //                                data = s.Element("returns") == null ? string.Empty : s.Element("returns").Value
            //                            };


            //                string ClassType = string.Empty;

            //                if (nodes != null)
            //                {

            //                    if (nodes.Count() > 0)
            //                        ClassType = nodes.ElementAt(0).Href.Replace("T:", "");

            //                }

            //                APIMem.ReturnsObject = ClassType;

            //                string ReturnText = string.Empty;

            //                foreach (var eachRetNode in each.Element("returns").Nodes())
            //                {

            //                    if (eachRetNode.ToString().Contains("<"))
            //                        ReturnText += "{object}";
            //                    else
            //                        ReturnText += eachRetNode.ToString();

            //                }


            //                APIMem.Returns = ReturnText;
            //                #endregion
            //            }
            //            var elements = each.Elements();

            //            foreach (var eachElem in elements)
            //            {

            //                WebThreadHelper.UpdateProgress(string.Format("Extracting Method Ramarks of {0} <br/>", methodName));

            //                if (eachElem.Name.ToString() == "param")
            //                    APIMem.AddParameterstoList(eachElem.Attribute("name").Value, eachElem.Value);


            //                if (eachElem.Name.ToString() == "p")
            //                {
            //                    APIMem.AddRemarks(eachElem.Value);

            //                }

            //            }


            //            WebThreadHelper.UpdateProgress(string.Format("Adding Method  {0} to Class {1} <br/>", methodName, className));
            //            ParsingHelper.AddClassMembers(nameSpace, className, APIMem, ref AsposeProductAPI);

            //            #endregion

            //        }

            //        catch (Exception ex)
            //        {
            //            //Log Ex Continue Extracting Next Node;
            //        }

            //    }

            //    if (TypeName.Contains("M:"))
            //    {
            //        try
            //        {

            //            #region Method Extractor
            //            string nameSpace = string.Empty;
            //            string className = string.Empty;
            //            string methodName = string.Empty;
            //            string MethodSignature = string.Empty;


            //            ParsingHelper.ExtractMethod(TypeName, ref nameSpace, ref className, ref methodName, ref MethodSignature);

            //            if (className.Contains("Calendar"))
            //            {

            //            }

            //            // var classReflection = LoadedAssembly.GetTypes().SingleOrDefault(s => s.FullName == nameSpace + "." + className);
            //            bool useMethod = true;

            //            //if (classReflection != null)
            //            //{
            //            //    bool isFound = false;
            //            //    string MTH = methodName.Replace("#", "");
            //            //    string sig = MethodSignature.Replace(" ", "");

            //            //    string compare = MTH + sig;


            //            //    if (methodName.Contains("ctor"))
            //            //    {
            //            //        ConstructorInfo[] info = classReflection.GetConstructors();

            //            //        if (info != null)
            //            //            foreach (var eachM in info)
            //            //            {
            //            //                string DllMethod = eachM.ToString().Replace(" ", "");
            //            //                int indexOf = DllMethod.IndexOf('.');
            //            //                indexOf++;


            //            //                DllMethod = DllMethod.Substring(indexOf, DllMethod.Length - indexOf);


            //            //                if (DllMethod == compare)
            //            //                {
            //            //                    isFound = true;
            //            //                    break;
            //            //                }
            //            //            }


            //            //        useMethod = isFound;
            //            //    }
            //            //    else
            //            //    {
            //            //        MethodInfo[] info = classReflection.GetMethods();
            //            //        if (info != null)
            //            //            foreach (var eachM in info)
            //            //            {

            //            //                bool isHid = IsHiding(eachM);
            //            //                bool isOverRid = IsOverriding(eachM);

            //            //                if (eachM.Name == methodName)
            //            //                    if (!eachM.IsPublic)
            //            //                        useMethod = false;


            //            //            }
            //            //    }


            //            //}

            //            if (!useMethod)
            //                continue;
            //            WebThreadHelper.UpdateProgress(string.Format("Extracting Method({0}) <br/>", methodName));

            //            APIClassMembers APIMem = new APIClassMembers();


            //            TypeName = TypeName.Replace("M:", "");

            //            if (methodName.Contains("#ctor"))
            //            {
            //                string conStructorName = string.Empty;

            //                if (MethodSignature == string.Empty)
            //                {
            //                    conStructorName = methodName.Replace("#ctor", " Constructor");
            //                    APIMem.Name = className + conStructorName;
            //                }
            //                else
            //                {
            //                    conStructorName = methodName.Replace("#ctor", " Constructor");
            //                    APIMem.Name = className + conStructorName;
            //                    conStructorName += "(" + MethodSignature;
            //                    APIMem.OverLoadName = conStructorName;


            //                }


            //                APIMem.isConstructor = true;


            //            }
            //            else
            //            {
            //                APIMem.isConstructor = false;
            //                APIMem.Name = methodName;
            //            }


            //            //APIMem.Description = Descrtiption;
            //            APIMem.Signature = MethodSignature;


            //            if (each.Element("example") != null)
            //            {
            //                APIMem.Example = each.Element("example").Value;

            //                var nodes = from s in each.Element("example").DescendantsAndSelf()
            //                            select new
            //                            {
            //                                data = s.Value,
            //                                code = s.Element("code") == null ? string.Empty : s.Element("code").Value
            //                            };

            //                string ExampleTextMethod = string.Empty;
            //                string CodeTextMethod = string.Empty;

            //                foreach (var eachExamples in nodes)
            //                {
            //                    ExampleTextMethod = eachExamples.data;
            //                    ExampleCode = eachExamples.code;
            //                    break;


            //                }


            //                ExampleTextMethod = ExampleTextMethod.Replace(ExampleCode, "");

            //                APIMem.Example = ExampleTextMethod;
            //                APIMem.CodeText = ExampleCode;
            //            }






            //            if (each.Element("summary") != null)
            //            {

            //                WebThreadHelper.UpdateProgress(string.Format("Extracting Method Summary of {0} <br/>", methodName));
            //                #region summary extractor
            //                var nodes = from s in each.Element("summary").Descendants()
            //                            select new
            //                            {
            //                                Href = s.Attribute("cref") == null ? string.Empty : s.Attribute("cref").Value,
            //                                data = s.Element("summary") == null ? string.Empty : s.Element("summary").Value
            //                            };


            //                string ClassType = string.Empty;

            //                if (nodes != null)
            //                {
            //                    if (nodes.Count() > 0)
            //                        ClassType = nodes.ElementAt(0).Href.Replace("T:", "");

            //                }

            //                APIMem.InitObject = ClassType;

            //                string SummaryText = string.Empty;

            //                foreach (var eachRetNode in each.Element("summary").Nodes())
            //                {

            //                    if (eachRetNode.ToString().Contains("<"))
            //                        SummaryText += "{object}";
            //                    else
            //                        SummaryText += eachRetNode.ToString();

            //                }


            //                APIMem.Description = SummaryText;
            //                #endregion
            //            }


            //            if (each.Element("returns") != null)
            //            {
            //                if (methodName.Contains("EnumerateMapiMessages"))
            //                {

            //                }

            //                WebThreadHelper.UpdateProgress(string.Format("Extracting Return Values of {0} <br/>", methodName));
            //                #region Returns
            //                var nodes = from s in each.Element("returns").Descendants()
            //                            select new
            //                            {
            //                                Href = s.Attribute("cref") == null ? string.Empty : s.Attribute("cref").Value,
            //                                data = s.Element("returns") == null ? string.Empty : s.Element("returns").Value
            //                            };


            //                string ClassType = string.Empty;

            //                if (nodes != null)
            //                {

            //                    if (nodes.Count() > 0)
            //                        ClassType = nodes.ElementAt(0).Href.Replace("T:", "");

            //                }

            //                APIMem.ReturnsObject = ClassType;

            //                string ReturnText = string.Empty;

            //                foreach (var eachRetNode in each.Element("returns").Nodes())
            //                {

            //                    if (eachRetNode.ToString().Contains("<"))
            //                        ReturnText += "{object}";
            //                    else
            //                        ReturnText += eachRetNode.ToString();

            //                }


            //                APIMem.Returns = ReturnText;
            //                #endregion
            //            }
            //            var elements = each.Elements();

            //            foreach (var eachElem in elements)
            //            {

            //                WebThreadHelper.UpdateProgress(string.Format("Extracting Method Ramarks of {0} <br/>", methodName));

            //                if (eachElem.Name.ToString() == "param")
            //                    APIMem.AddParameterstoList(eachElem.Attribute("name").Value, eachElem.Value);


            //                if (eachElem.Name.ToString() == "p")
            //                {
            //                    APIMem.AddRemarks(eachElem.Value);

            //                }

            //            }


            //            WebThreadHelper.UpdateProgress(string.Format("Adding Method  {0} to Class {1} <br/>", methodName, className));
            //            ParsingHelper.AddClassMembers(nameSpace, className, APIMem, ref AsposeProductAPI);

            //            #endregion

            //        }

            //        catch (Exception ex)
            //        {
            //            //Log Ex Continue Extracting Next Node;
            //        }
            //    }

            //    if (TypeName.Contains("F:"))
            //    {
            //        try
            //        {
            //            #region Format Extractor
            //            string className = string.Empty;
            //            string propertyName = string.Empty;
            //            string nameSp = string.Empty;
            //            TypeName = TypeName.Replace("F:", "");
            //            ParsingHelper.ExtractProperties(ref nameSp, TypeName, ref className, ref propertyName);

            //            WebThreadHelper.UpdateProgress(string.Format("Extracting Enumeration {0} <br/>", propertyName));

            //            APIClassFormat myFormat = new APIClassFormat();




            //            if (each.Element("summary") != null)
            //            {
            //                #region summary extractor
            //                var nodes = from s in each.Element("summary").Descendants()
            //                            select new
            //                            {
            //                                Href = s.Attribute("cref") == null ? string.Empty : s.Attribute("cref").Value,
            //                                data = s.Element("summary") == null ? string.Empty : s.Element("summary").Value
            //                            };


            //                string ClassType = string.Empty;

            //                if (nodes.Count() > 0)
            //                {
            //                    if (nodes != null)
            //                    {
            //                        ClassType = nodes.ElementAt(0).Href.Replace("F:", "");

            //                    }
            //                    myFormat.EqualsType = ClassType;
            //                }



            //                string SummaryText = string.Empty;

            //                foreach (var eachRetNode in each.Element("summary").Nodes())
            //                {

            //                    if (eachRetNode.ToString().Contains("<"))
            //                        SummaryText += "{object}";
            //                    else
            //                        SummaryText += eachRetNode.ToString();

            //                }


            //                myFormat.Summary = SummaryText;
            //                myFormat.Name = propertyName;




            //                #endregion
            //            }


            //            WebThreadHelper.UpdateProgress(string.Format("Adding  Enumeration {0} to Class {1} <br/>", propertyName, className));

            //            myFormat.Name = propertyName;

            //            ParsingHelper.AddClassFormats(nameSp, className, myFormat, ref AsposeProductAPI);

            //            #endregion
            //        }

            //        catch (Exception ex)
            //        {
            //            //Log Ex Continue Extracting Next Node;
            //        }

            //    }



            //    #endregion
            //}

        }


        public static bool StartParsing(string spaceKey, string fileName, ref string ResultCode, string AssemblyName)
        {
            Assembly LoadedAssembly = null;

           
            try
            {

                if (AssemblyName != string.Empty)
                {
                    try
                    {
                        LoadedAssembly = Assembly.LoadFile(HttpContext.Current.Server.MapPath("~/Temp/" + AssemblyName));

                    }

                    catch (Exception ex)
                    {


                        WebThreadHelper.Error("Error Loading Assembly,  Error=" + ex.Message);
                        WebThreadHelper.Error(" <br/>");
                    }



                }


                XDocument xdoc = XDocument.Load(HttpContext.Current.Server.MapPath("~/XMl/" + fileName));


                var assembly = from item in xdoc.Descendants("assembly")
                               select new
                             {
                                 Name = item.Element("name").IsEmpty ? string.Empty : item.Element("name").Value
                             }
                                    ;


                if (assembly == null) { ResultCode = "Not a Valid Schema"; return false; }


                XMLParser.Models.ProductAPI AsposeProductAPI = new Models.ProductAPI();
                AsposeProductAPI.SpaceKey = spaceKey;
                AsposeProductAPI.AssemblyName = assembly.ElementAt(0).Name;


                APINameSpace nsAspose = new APINameSpace();
                nsAspose.Title = AsposeProductAPI.AssemblyName;
                AsposeProductAPI.AddNameSpace(nsAspose);

                WebThreadHelper.UpdateProgress(string.Format("Dll Parsing Started for  {0} <br/>", AsposeProductAPI.AssemblyName));

                ParseDllFirst(LoadedAssembly, ref AsposeProductAPI);

                

                WebThreadHelper.UpdateProgress(string.Format("XML Parsing Started for  {0} <br/>", AsposeProductAPI.AssemblyName));
                try
                {
                    ParseXMLCommentsFile(xdoc, ref AsposeProductAPI);

                    FillMissingDescriptionFromParent(ref AsposeProductAPI);
                }

                catch (Exception ex)
                {


                }


                WebThreadHelper.UpdateProgress(" <br/><hr/>Parsing has been Completed.<br/>");

                WebThreadHelper.UpdateProgress(" Exporting Data to Confluence.<br/>");

                ProductAPI res = StructureCleaner.Clean(AsposeProductAPI);
                ConflunceExporter.ExporttoConfluence(res, spaceKey);
                WebThreadHelper.UpdateProgress(" Exporting Completed <br/>");


            }// End of Try 

            catch (Exception ex)
            {
                if (ex is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = ex as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;

                    //foreach (var each in loaderExceptions)
                    //{
                    //    WebThreadHelper.UpdateProgress(" Obfuscated Excpetion =" + each);
                    //    WebThreadHelper.UpdateProgress(" <br/>");
                    //}
                }

                WebThreadHelper.UpdateProgress(" Error=" + ex.Message);
                WebThreadHelper.UpdateProgress(" <br/>");

                return false ;


            }



            ResultCode = "Completed";
            return true;

        }

        public static string GetDescriptionFromParent(string  baseType, string PropertyName,ref ProductAPI API)
        {

            APIClass clsBase = null;

            if(API.lstNameSpaces!=null)
            foreach (var eachN in API.lstNameSpaces)
            {

                clsBase = eachN.lstClasses.SingleOrDefault(s => s.ClassName == baseType);

                if (clsBase != null)
                    break;

            }

            if (clsBase == null) return string.Empty;


            if(clsBase.lstProperties!=null)
           
            {
                APIClassProperties prop = clsBase.lstProperties.SingleOrDefault(s => s.Alias == PropertyName);

                return prop==null?"":prop.Description;

            }

            return string.Empty;
        }

        public static void FillMissingDescriptionFromParent(ref ProductAPI API )
        {

            foreach (var eachN in API.lstNameSpaces)
            {
                if(eachN.lstClasses!=null)
                foreach (var eachC in eachN.lstClasses)
                {

                    if(eachC.lstProperties!=null)
                    foreach (var eachP in eachC.lstProperties)
                    {

                        if (string.IsNullOrEmpty(eachP.Description)&&eachC.lstBaseTypes.Count>1)
                        {
                            //string baseName= eachC.lstBaseTypes.FirstOrDefault().Value;

                            foreach (var eachB in eachC.lstBaseTypes)
                            {
                                string fullName = eachB.Key;
                                string basename = eachB.Value;

                                if (!fullName.Contains("Aspose"))
                                    continue;

                                try
                                {

                                    string getParentDescription = GetDescriptionFromParent(basename, eachP.Name, ref API);


                                    if (!string.IsNullOrEmpty(getParentDescription) )
                                    {
                                        eachP.Description = getParentDescription;
                                        break;

                                    }



                                }
                                catch (Exception ex)
                                {
                                }

                            }

                        }

                    }


                }


            }




        }

        public static void  StartParsingMultiple(string spaceKey, List<string> xmlFiles, List<string> AssemblyFiles, ref string ResultCode)
        {

            Assembly LoadedAssembly = null;

            List<string> lstAssembly = new List<string>();

            try
            {
                #region Iterate Assembly

                if (xmlFiles.Count != AssemblyFiles.Count) return;

                XMLParser.Models.ProductAPI AsposeProductAPI = new Models.ProductAPI();
                AsposeProductAPI.SpaceKey = spaceKey;
              

                for (int i = 0; i < AssemblyFiles.Count; i++)
                {

                    if (AssemblyFiles[i] != string.Empty)
                    {
                        try
                        {
                            LoadedAssembly = Assembly.LoadFile(HttpContext.Current.Server.MapPath("~/Temp/" + AssemblyFiles[i]));

                        }

                        catch (Exception ex)
                        {


                            WebThreadHelper.Error("Error Loading Assembly,  Error=" + ex.Message);
                            WebThreadHelper.Error(" <br/>");
                        }



                    }


                    XDocument xdoc = XDocument.Load(HttpContext.Current.Server.MapPath("~/XMl/" + xmlFiles[i]));


                    var assembly = from item in xdoc.Descendants("assembly")
                                   select new
                                   {
                                       Name = item.Element("name").IsEmpty ? string.Empty : item.Element("name").Value
                                   }
                                        ;


                    if (assembly == null) { ResultCode = "Not a Valid Schema"; return; }


                  


                    APINameSpace nsAspose = new APINameSpace();
                    nsAspose.Title = assembly.ElementAt(0).Name;
                   
                    AsposeProductAPI.AddNameSpace(nsAspose);

                    AsposeProductAPI.AssemblyName = assembly.ElementAt(0).Name;

                    lstAssembly.Add(AsposeProductAPI.AssemblyName);

                    WebThreadHelper.UpdateProgress(string.Format("Dll Parsing Started for  {0} <br/>", AsposeProductAPI.AssemblyName));

                    ParseDllFirst(LoadedAssembly, ref AsposeProductAPI);

                    WebThreadHelper.UpdateProgress(string.Format("XML Parsing Started for  {0} <br/>", AsposeProductAPI.AssemblyName));

                    ParseXMLCommentsFile(xdoc, ref AsposeProductAPI);


                    WebThreadHelper.UpdateProgress(" <br/><hr/>Parsing has been Completed.<br/>");

                    WebThreadHelper.UpdateProgress(" Exporting Data to Confluence.<br/>");
                }

                #endregion
                ConflunceExporter.ExporttoConfluence(AsposeProductAPI, spaceKey,lstAssembly);
                WebThreadHelper.UpdateProgress(" Exporting Completed <br/>");


            }// End of Try 

            catch (Exception ex)
            {
                if (ex is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = ex as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;

                    foreach (var each in loaderExceptions)
                    {
                        WebThreadHelper.UpdateProgress(" Error=" + each.Message);
                        WebThreadHelper.UpdateProgress(" <br/>");
                    }
                }

                WebThreadHelper.UpdateProgress(" Error=" + ex.Message);
                WebThreadHelper.UpdateProgress(" <br/>");

                return ;


            }



            ResultCode = "Completed";


        }
    }


  
   

}
