using System;
using System.Collections.Generic;
using System.Linq;

namespace ZpqrtBnk.ModelsBuilder.Building
{
    /// <summary>
    /// Contains the result of a code parsing.
    /// </summary>
    public class ParseResult
    {
        // "alias" is the umbraco alias
        // content "name" is the complete name eg Foo.Bar.Name
        // property "name" is just the local name

        // see notes in IgnoreContentTypeAttribute

        private readonly HashSet<string> _ignoredContent 
            = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        //private readonly HashSet<string> _ignoredMixin
        //    = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        //private readonly HashSet<string> _ignoredMixinProperties
        //    = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, string> _renamedContent 
            = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly HashSet<string> _withImplementContent
            = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, HashSet<string>> _ignoredProperty
            = new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, Dictionary<string, string>> _renamedProperty
            = new Dictionary<string, Dictionary<string, string>>();
        private readonly Dictionary<string, string> _contentBase
            = new Dictionary<string, string>();
        private readonly Dictionary<string, string[]> _contentInterfaces 
            = new Dictionary<string, string[]>();
        private readonly List<string> _usingNamespaces
            = new List<string>();
        private readonly HashSet<string> _withCtor
            = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, List<string>> _implementedExtensions
            = new Dictionary<string, List<string>>();
        private readonly List<ModelsBaseClassInfo> _modelsBaseClassNames
             = new List<ModelsBaseClassInfo>();

        private string _modelInfosClassNamespace;

        public static readonly ParseResult Empty = new ParseResult();

        private class ModelsBaseClassInfo
        {
            public ModelsBaseClassInfo(bool isContent, string aliasPattern, string baseClassName)
            {
                IsContent = isContent;
                AliasPattern = aliasPattern;
                BaseClassName = baseClassName;
            }

            public bool IsElement => !IsContent;

            public bool IsContent { get; }

            public string AliasPattern { get; }

            public string BaseClassName { get; }
        }

        #region Declare

        // content with that alias should not be generated
        // alias can end with a * (wildcard)
        public void SetIgnoredContent(string contentAlias /*, bool ignoreContent, bool ignoreMixin, bool ignoreMixinProperties*/)
        {
            //if (ignoreContent)
                _ignoredContent.Add(contentAlias);
            //if (ignoreMixin)
            //    _ignoredMixin.Add(contentAlias);
            //if (ignoreMixinProperties)
            //    _ignoredMixinProperties.Add(contentAlias);
        }

        // content with that alias should be generated with a different name
        public void SetRenamedContent(string contentAlias, string contentName, bool withImplement)
        {
            _renamedContent[contentAlias] = contentName;
            if (withImplement)
                _withImplementContent.Add(contentAlias);
        }

        // property with that alias should not be generated
        // applies to content name and any content that implements it
        // here, contentName may be an interface
        // alias can end with a * (wildcard)
        public void SetIgnoredProperty(string contentName, string propertyAlias)
        {
            HashSet<string> ignores;
            if (!_ignoredProperty.TryGetValue(contentName, out ignores))
                ignores = _ignoredProperty[contentName] = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            ignores.Add(propertyAlias);
        }

        // property with that alias should be generated with a different name
        // applies to content name and any content that implements it
        // here, contentName may be an interface
        public void SetRenamedProperty(string contentName, string propertyAlias, string propertyName)
        {
            Dictionary<string, string> renames;
            if (!_renamedProperty.TryGetValue(contentName, out renames))
                renames = _renamedProperty[contentName] = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            renames[propertyAlias] = propertyName;
        }

        // content with that name has a base class so no need to generate one
        public void SetContentBaseClass(string contentName, string baseName)
        {
            if (baseName.ToLowerInvariant() != "object")
                _contentBase[contentName] = baseName;
        }

        // content with that name implements the interfaces
        public void SetContentInterfaces(string contentName, IEnumerable<string> interfaceNames)
        {
            _contentInterfaces[contentName] = interfaceNames.ToArray();
        }

        public void SetModelsBaseClassName(bool isContent, string aliasPattern, string baseClassName)
        {
            _modelsBaseClassNames.Add(new ModelsBaseClassInfo(isContent, aliasPattern, baseClassName));
        }

        public void SetModelsNamespace(string modelsNamespace)
        {
            ModelsNamespace = modelsNamespace;
        }

        public void SetUsingNamespace(string usingNamespace)
        {
            _usingNamespaces.Add(usingNamespace);
        }

        public void SetHasCtor(string contentName)
        {
            _withCtor.Add(contentName);
        }

        public void SetImplementedExtension(string typeName, string propertyName)
        {
            // see CodeParser
            // typeName here... can be 'Foo' or 'Namespace.Foo' and we want to keep the last part
            var pos = typeName.LastIndexOf('.');
            if (pos > 0) typeName = typeName.Substring(pos + 1);

            if (!_implementedExtensions.ContainsKey(typeName))
                _implementedExtensions[typeName] = new List<string>();

            _implementedExtensions[typeName].Add(propertyName);
        }

        public void SetGeneratePropertyGetters(bool value)
        {
            GeneratePropertyGetters = value;
        }

        public void SetGenerateFallbackFuncExtensionMethods(bool value)
        {
            GenerateFallbackFuncExtensionMethods = value;
        }

        public void SetModelInfosClassName(string value)
        {
            ModelInfoClassName = string.IsNullOrWhiteSpace(value) ? "ModelInfos" : value;
        }

        public void SetModelInfosClassNamespace(string value)
        {
            _modelInfosClassNamespace = string.IsNullOrWhiteSpace(value) ? null : value;
        }

        public void SetTypeModelPrefix(string value)
        {
            TypeModelPrefix = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
        }

        public void SetTypeModelSuffix(string value)
        {
            TypeModelSuffix = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
        }

        #endregion

        #region Query

        public bool IsIgnored(string contentAlias)
        {
            return IsContentOrMixinIgnored(contentAlias, _ignoredContent);
        }

        //public bool IsMixinIgnored(string contentAlias)
        //{
        //    return IsContentOrMixinIgnored(contentAlias, _ignoredMixin);
        //}
        
        //public bool IsMixinPropertiesIgnored(string contentAlias)
        //{
        //    return IsContentOrMixinIgnored(contentAlias, _ignoredMixinProperties);
        //}

        private static bool IsContentOrMixinIgnored(string contentAlias, HashSet<string> ignored)
        {
            if (ignored.Contains(contentAlias)) return true;
            return ignored
                .Where(x => x.EndsWith("*"))
                .Select(x => x.Substring(0, x.Length - 1))
                .Any(x => contentAlias.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool HasContentBase(string contentName)
        {
            return _contentBase.ContainsKey(contentName);
        }

        public bool IsContentRenamed(string contentAlias)
        {
            return _renamedContent.ContainsKey(contentAlias);
        }

        public bool HasContentImplement(string contentAlias)
        {
            return _withImplementContent.Contains(contentAlias);
        }

        public string ContentClrName(string contentAlias)
        {
            string name;
            return (_renamedContent.TryGetValue(contentAlias, out name)) ? name : null;
        }

        public bool IsPropertyIgnored(string contentName, string propertyAlias)
        {
            HashSet<string> ignores;
            if (_ignoredProperty.TryGetValue(contentName, out ignores))
            {
                if (ignores.Contains(propertyAlias)) return true;
                if (ignores
                        .Where(x => x.EndsWith("*"))
                        .Select(x => x.Substring(0, x.Length - 1))
                        .Any(x => propertyAlias.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
                    return true;
            }
            string baseName;
            if (_contentBase.TryGetValue(contentName, out baseName)
                && IsPropertyIgnored(baseName, propertyAlias)) return true;
            string[] interfaceNames;
            if (_contentInterfaces.TryGetValue(contentName, out interfaceNames)
                && interfaceNames.Any(interfaceName => IsPropertyIgnored(interfaceName, propertyAlias))) return true;
            return false;
        }

        public string PropertyClrName(string contentName, string propertyAlias)
        {
            Dictionary<string, string> renames;
            string name;
            if (_renamedProperty.TryGetValue(contentName, out renames)
                && renames.TryGetValue(propertyAlias, out name)) return name;
            string baseName;
            if (_contentBase.TryGetValue(contentName, out baseName)
                && null != (name = PropertyClrName(baseName, propertyAlias))) return name;
            string[] interfaceNames;
            if (_contentInterfaces.TryGetValue(contentName, out interfaceNames)
                && null != (name = interfaceNames
                    .Select(interfaceName => PropertyClrName(interfaceName, propertyAlias))
                    .FirstOrDefault(x => x != null))) return name;
            return null;
        }

        public string GetModelBaseClassName(bool isContent, string alias)
        {
            bool Match(string pattern, string s)
            {
                if (pattern == "*") return true;
                if (pattern.StartsWith("*")) return s.EndsWith(pattern.Substring(1));
                if (pattern.EndsWith("*")) return s.StartsWith(pattern.Substring(0, pattern.Length - 1));
                return pattern == s;
            }

            var infos = _modelsBaseClassNames.Where(x => x.IsContent == isContent);
            infos = infos.OrderByDescending(x => x.AliasPattern); // longest first... so at least '*' triggers last
            var info = infos.FirstOrDefault(x => Match(x.AliasPattern, alias));
            return info?.BaseClassName;
        }

        public bool HasModelsNamespace
        {
            get { return !string.IsNullOrWhiteSpace(ModelsNamespace); }
        }

        public string ModelsNamespace { get; private set; }

        public bool GeneratePropertyGetters { get; private set; }

        public bool GenerateFallbackFuncExtensionMethods { get; private set; }

        public IEnumerable<string> UsingNamespaces
        {
            get { return _usingNamespaces; }
        }

        public bool HasCtor(string contentName)
        {
            return _withCtor.Contains(contentName);
        }

        public bool IsExtensionImplemented(string typeFullName, string propertyClrName)
        {
            return _implementedExtensions.TryGetValue(typeFullName, out var props) && props.Contains(propertyClrName);
        }

        public string ModelInfoClassName { get; private set; } = "ModelInfos";

        public string ModelInfoClassNamespace => _modelInfosClassNamespace ?? ModelsNamespace ?? "Umbraco.Web.PublishedModels"; // FIXME

        public string TypeModelPrefix { get; private set; } = "";

        public string TypeModelSuffix { get; private set; } = "";

        #endregion
    }
}