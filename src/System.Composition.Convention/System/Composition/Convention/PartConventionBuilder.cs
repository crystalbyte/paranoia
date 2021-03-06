﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.Composition.Diagnostics;
using Microsoft.Internal;

namespace System.Composition.Convention
{
    /// <summary>
    /// Configures a type as a MEF part.
    /// </summary>
    public class PartConventionBuilder
    {
        private Type[] _emptyTypeArray = new Type[0];
        private static List<Attribute> _onImportsSatisfiedAttributeList;
        private readonly static List<Attribute> ImportingConstructorList = new List<Attribute>() { new ImportingConstructorAttribute() };
        private readonly static  Type ExportAttributeType = typeof(ExportAttribute);
        private List<ExportConventionBuilder> _typeExportBuilders;
        private List<ImportConventionBuilder> _constructorImportBuilders;
        private bool _isShared;
        private string _sharingBoundary;

        // Metadata selection
        private List<Tuple<string, object>> _metadataItems;
        private List<Tuple< string, Func<Type, object>>> _metadataItemFuncs;

        // Constructor selector / configuration
        private Func<IEnumerable<ConstructorInfo>, ConstructorInfo> _constructorFilter;
        private Action<ParameterInfo, ImportConventionBuilder> _configureConstuctorImports;

        //Property Import/Export selection and configuration
        private List<Tuple<Predicate<PropertyInfo>, Action<PropertyInfo, ExportConventionBuilder>, Type>> _propertyExports;
        private List<Tuple<Predicate<PropertyInfo>, Action<PropertyInfo, ImportConventionBuilder>>> _propertyImports;
        private List<Tuple<Predicate<Type>, Action<Type, ExportConventionBuilder>>> _interfaceExports;
        private List<Predicate<MethodInfo>> _methodImportsSatisfiedNotifications;

        internal Predicate<Type> SelectType { get; private set; }

        internal PartConventionBuilder(Predicate<Type> selectType)
        {
            this.SelectType = selectType;
            this._typeExportBuilders = new List<ExportConventionBuilder>();
            this._constructorImportBuilders = new List<ImportConventionBuilder>();
            this._propertyExports = new List<Tuple<Predicate<PropertyInfo>, Action<PropertyInfo, ExportConventionBuilder>, Type>>();
            this._propertyImports = new List<Tuple<Predicate<PropertyInfo>, Action<PropertyInfo, ImportConventionBuilder>>>();
            this._interfaceExports = new List<Tuple<Predicate<Type>, Action<Type, ExportConventionBuilder>>>();
            this._methodImportsSatisfiedNotifications = new List<Predicate<MethodInfo>>();
        }

        /// <summary>
        /// Export the part using its own concrete type as the contract.
        /// </summary>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder Export()
        {
            var exportBuilder = new ExportConventionBuilder();
            this._typeExportBuilders.Add(exportBuilder);
            return this;
        }

        /// <summary>
        /// Export the part.
        /// </summary>
        /// <param name="exportConfiguration">Configuration action for the export.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder Export(Action<ExportConventionBuilder> exportConfiguration)
        {
            Requires.NotNull(exportConfiguration, "exportConfiguration");
            var exportBuilder = new ExportConventionBuilder();
            exportConfiguration(exportBuilder);
            this._typeExportBuilders.Add(exportBuilder);
            return this;
        }

        /// <summary>
        /// Export the part using <typeparamref name="T"/> as the contract.
        /// </summary>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder Export<T>()
        {
            var exportBuilder = new ExportConventionBuilder().AsContractType<T>();
            this._typeExportBuilders.Add(exportBuilder);
            return this;
        }

        /// <summary>
        /// Export the class using <typeparamref name="T"/> as the contract.
        /// </summary>
        /// <param name="exportConfiguration">Configuration action for the export.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder Export<T>(Action<ExportConventionBuilder> exportConfiguration)
        {
            Requires.NotNull(exportConfiguration, "exportConfiguration");
            var exportBuilder = new ExportConventionBuilder().AsContractType<T>();
            exportConfiguration(exportBuilder);
            this._typeExportBuilders.Add(exportBuilder);
            return this;
        }

        /// <summary>
        /// Select which of the available constructors will be used to instantiate the part.
        /// </summary>
        /// <param name="constructorSelector">Filter that selects a single constructor.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder SelectConstructor(Func<IEnumerable<ConstructorInfo>, ConstructorInfo> constructorSelector)
        {
            Requires.NotNull(constructorSelector, "constructorSelector");
            this._constructorFilter = constructorSelector;
            return this;
        }

        /// <summary>
        /// Select which of the available constructors will be used to instantiate the part.
        /// </summary>
        /// <param name="constructorSelector">Filter that selects a single constructor.</param>
        /// <param name="importConfiguration">Action configuring the parameters of the selected constructor.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder SelectConstructor(
            Func<IEnumerable<ConstructorInfo>, ConstructorInfo> constructorSelector, 
            Action<ParameterInfo, ImportConventionBuilder> importConfiguration)
        {
            Requires.NotNull(importConfiguration, "importConfiguration");
            SelectConstructor(constructorSelector);
            this._configureConstuctorImports = importConfiguration;
            return this;
        }

        /// <summary>
        /// Select the interfaces on the part type that will be exported.
        /// </summary>
        /// <param name="interfaceFilter">Filter for interfaces.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder ExportInterfaces(Predicate<Type> interfaceFilter)
        {
            Requires.NotNull(interfaceFilter, "interfaceFilter");
            return ExportInterfacesImpl(interfaceFilter, null);
        }

        /// <summary>
        /// Export all interfaces implemented by the part.
        /// </summary>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder ExportInterfaces()
        {
            return ExportInterfaces(t => true);
        }

        /// <summary>
        /// Select the interfaces on the part type that will be exported.
        /// </summary>
        /// <param name="interfaceFilter">Filter for interfaces.</param>
        /// <param name="exportConfiguration">Action to configure selected interfaces.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder ExportInterfaces(
            Predicate<Type> interfaceFilter,
            Action<Type, ExportConventionBuilder> exportConfiguration)
        {
            Requires.NotNull(interfaceFilter, "interfaceFilter");
            Requires.NotNull(exportConfiguration, "exportConfiguration");
            return ExportInterfacesImpl(interfaceFilter, exportConfiguration);
        }

        PartConventionBuilder ExportInterfacesImpl(
            Predicate<Type> interfaceFilter,
            Action<Type, ExportConventionBuilder> exportConfiguration)
        {
            this._interfaceExports.Add(Tuple.Create(interfaceFilter, exportConfiguration));
            return this;
        }

        /// <summary>
        /// Select properties on the part to export.
        /// </summary>
        /// <param name="propertyFilter">Selector for exported properties.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder ExportProperties(Predicate<PropertyInfo> propertyFilter)
        {
            Requires.NotNull(propertyFilter, "propertyFilter");

            return ExportPropertiesImpl(propertyFilter, null);
        }

        /// <summary>
        /// Select properties on the part to export.
        /// </summary>
        /// <param name="propertyFilter">Selector for exported properties.</param>
        /// <param name="exportConfiguration">Action to configure selected properties.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder ExportProperties(
            Predicate<PropertyInfo> propertyFilter,
            Action<PropertyInfo, ExportConventionBuilder> exportConfiguration)
        {
            Requires.NotNull(propertyFilter, "propertyFilter");
            Requires.NotNull(exportConfiguration, "exportConfiguration");
            return ExportPropertiesImpl(propertyFilter, exportConfiguration);
        }

        PartConventionBuilder ExportPropertiesImpl(
            Predicate<PropertyInfo> propertyFilter, 
            Action<PropertyInfo, ExportConventionBuilder> exportConfiguration)
        {            
            this._propertyExports.Add(Tuple.Create(propertyFilter, exportConfiguration, default(Type)));
            return this;
        }

        /// <summary>
        /// Select properties to export from the part.
        /// </summary>
        /// <typeparam name="T">Contract type to export.</typeparam>
        /// <param name="propertyFilter">Filter to select matching properties.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder ExportProperties<T>(Predicate<PropertyInfo> propertyFilter)
        {
            Requires.NotNull(propertyFilter, "propertyFilter");

            return ExportPropertiesImpl<T>(propertyFilter, null);
        }

        /// <summary>
        /// Select properties to export from the part.
        /// </summary>
        /// <typeparam name="T">Contract type to export.</typeparam>
        /// <param name="propertyFilter">Filter to select matching properties.</param>
        /// <param name="exportConfiguration">Action to configure selected properties.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder ExportProperties<T>(
            Predicate<PropertyInfo> propertyFilter,
            Action<PropertyInfo, ExportConventionBuilder> exportConfiguration)
        {
            Requires.NotNull(propertyFilter, "propertyFilter");
            Requires.NotNull(exportConfiguration, "exportConfiguration");

            return ExportPropertiesImpl<T>(propertyFilter, exportConfiguration);
        }

        PartConventionBuilder ExportPropertiesImpl<T>(
            Predicate<PropertyInfo> propertyFilter,
            Action<PropertyInfo, ExportConventionBuilder> exportConfiguration)
        {
            this._propertyExports.Add(Tuple.Create(propertyFilter, exportConfiguration, typeof(T)));
            return this;
        }

        /// <summary>
        /// Select properties to import into the part.
        /// </summary>
        /// <param name="propertyFilter">Filter to select matching properties.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder ImportProperties(Predicate<PropertyInfo> propertyFilter)
        {
            Requires.NotNull(propertyFilter, "propertyFilter");

            return ImportPropertiesImpl(propertyFilter, null);
        }

        /// <summary>
        /// Select properties to import into the part.
        /// </summary>
        /// <param name="propertyFilter">Filter to select matching properties.</param>
        /// <param name="importConfiguration">Action to configure selected properties.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder ImportProperties(
            Predicate<PropertyInfo> propertyFilter,
            Action<PropertyInfo, ImportConventionBuilder> importConfiguration)
        {
            Requires.NotNull(propertyFilter, "propertyFilter");
            Requires.NotNull(importConfiguration, "importConfiguration");

            return ImportPropertiesImpl(propertyFilter, importConfiguration);
        }

        PartConventionBuilder ImportPropertiesImpl(
            Predicate<PropertyInfo> propertyFilter,
            Action<PropertyInfo, ImportConventionBuilder> importConfiguration)
        {
            this._propertyImports.Add(Tuple.Create(propertyFilter, importConfiguration));
            return this;
        }

        /// <summary>
        /// Select properties to import into the part.
        /// </summary>
        /// <typeparam name="T">Property type to import.</typeparam>
        /// <param name="propertyFilter">Filter to select matching properties.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder ImportProperties<T>(Predicate<PropertyInfo> propertyFilter)
        {
            Requires.NotNull(propertyFilter, "propertyFilter");

            return ImportPropertiesImpl<T>(propertyFilter, null);
        }

        /// <summary>
        /// Select properties to import into the part.
        /// </summary>
        /// <typeparam name="T">Property type to import.</typeparam>
        /// <param name="propertyFilter">Filter to select matching properties.</param>
        /// <param name="importConfiguration">Action to configure selected properties.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder ImportProperties<T>(
            Predicate<PropertyInfo> propertyFilter,
            Action<PropertyInfo, ImportConventionBuilder> importConfiguration)
        {
            Requires.NotNull(propertyFilter, "propertyFilter");
            Requires.NotNull(importConfiguration, "importConfiguration");

            return ImportPropertiesImpl<T>(propertyFilter, importConfiguration);
        }

        PartConventionBuilder ImportPropertiesImpl<T>(
            Predicate<PropertyInfo> propertyFilter,
            Action<PropertyInfo, ImportConventionBuilder> importConfiguration)
        {
            Predicate<PropertyInfo> typedFilter = pi => pi.PropertyType.Equals(typeof(T)) && (propertyFilter == null || propertyFilter(pi));
            this._propertyImports.Add(Tuple.Create(typedFilter, importConfiguration));
            return this;
        }

        /// <summary>
        /// Mark the part as being shared within the entire composition.
        /// </summary>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder NotifyImportsSatisfied(Predicate<MethodInfo> methodFilter)
        {
            this._methodImportsSatisfiedNotifications.Add(methodFilter);
            return this;
        }
        
        /// <summary>
        /// Mark the part as being shared within the entire composition.
        /// </summary>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder Shared()
        {
            return SharedImpl(null);
        }

        /// <summary>
        /// Mark the part as being shared within the specified boundary.
        /// </summary>
        /// <param name="sharingBoundary">Name of the sharing boundary.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder Shared(string sharingBoundary)
        {
            Requires.NotNullOrEmpty(sharingBoundary, "sharingBoundary");
            return SharedImpl(sharingBoundary);
        }

        PartConventionBuilder SharedImpl(string sharingBoundary)
        {
            this._isShared = true;
            this._sharingBoundary = sharingBoundary;
            return this;
        }

        /// <summary>
        /// Add the specified metadata to the part.
        /// </summary>
        /// <param name="name">The metadata name.</param>
        /// <param name="value">The metadata value.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder AddPartMetadata(string name, object value)
        {
            Requires.NotNullOrEmpty(name, "name");

            if(this._metadataItems == null)
            {
                this._metadataItems = new List<Tuple<string, object>>();
            }
            this._metadataItems.Add(Tuple.Create(name, value));
            return this;
        }

        /// <summary>
        /// Add the specified metadata to the part.
        /// </summary>
        /// <param name="name">The metadata name.</param>
        /// <param name="getValueFromPartType">A function mapping the part type to the metadata value.</param>
        /// <returns>A part builder allowing further configuration of the part.</returns>
        public PartConventionBuilder AddPartMetadata(string name, Func<Type, object> getValueFromPartType)
        {
            Requires.NotNullOrEmpty(name, "name");
            Requires.NotNull(getValueFromPartType, "itemFunc");

            if (this._metadataItemFuncs == null)
            {
                this._metadataItemFuncs = new List<Tuple<string, Func<Type, object>>>();
            }
            this._metadataItemFuncs.Add(Tuple.Create(name, getValueFromPartType));
            return this;
        }

        static bool MemberHasExportMetadata(MemberInfo member)
        {
            foreach (var attr in member.GetAttributes<Attribute>())
            {
                var provider = attr as ExportMetadataAttribute;
                if (provider != null)
                {
                    return true;
                }
                else
                {
                    Type attrType = attr.GetType();
                    // Perf optimization, relies on short circuit evaluation, often a property attribute is an ExportAttribute
                    if (attrType != PartConventionBuilder.ExportAttributeType && attrType.GetTypeInfo().IsAttributeDefined<MetadataAttributeAttribute>(true))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal IEnumerable<Attribute> BuildTypeAttributes(Type type)
        {
            var attributes = new List<Attribute>();

            if(this._typeExportBuilders != null)
            {
                bool isConfigured = type.GetTypeInfo().GetFirstAttribute<ExportAttribute>() != null || MemberHasExportMetadata(type.GetTypeInfo());
                if(isConfigured)
                {
                    CompositionTrace.Registration_TypeExportConventionOverridden(type);
                }
                else
                {
                    foreach (var export in this._typeExportBuilders)
                    {
                        export.BuildAttributes(type, ref attributes);
                    }
                }
            }

            if (this._isShared)
            {
                // Check if there is already a SharedAttribute.  If found Trace a warning and do not add this Shared
                // otherwise add new one
                bool isConfigured = type.GetTypeInfo().GetFirstAttribute<SharedAttribute>() != null;
                if(isConfigured)
                {
                    CompositionTrace.Registration_PartCreationConventionOverridden(type);
                }
                else
                {
                    attributes.Add(this._sharingBoundary == null ?
                        new SharedAttribute() : 
                        new SharedAttribute(this._sharingBoundary));
                }
            }

            //Add metadata attributes from direct specification
            if (this._metadataItems != null)
            {
                bool isConfigured = type.GetTypeInfo().GetFirstAttribute<PartMetadataAttribute>() != null;
                if(isConfigured)
                {
                    CompositionTrace.Registration_PartMetadataConventionOverridden(type);
                }
                else
                {
                    foreach (var item in this._metadataItems)
                    {
                        attributes.Add(new PartMetadataAttribute(item.Item1, item.Item2));
                    }
                }
            }

            //Add metadata attributes from func specification
            if (this._metadataItemFuncs != null)
            {
                bool isConfigured = type.GetTypeInfo().GetFirstAttribute<PartMetadataAttribute>() != null;
                if(isConfigured)
                {
                    CompositionTrace.Registration_PartMetadataConventionOverridden(type);
                }
                else
                {
                    foreach (var item in this._metadataItemFuncs)
                    {
                        var name = item.Item1;
                        var value = (item.Item2 != null) ? item.Item2(type) : null;
                        attributes.Add(new PartMetadataAttribute(name, value));
                    }
                }
            }

            if (this._interfaceExports.Any())
            {
                if(this._typeExportBuilders != null)
                {
                    bool isConfigured = type.GetTypeInfo().GetFirstAttribute<ExportAttribute>() != null || MemberHasExportMetadata(type.GetTypeInfo());
                    if(isConfigured)
                    {
                        CompositionTrace.Registration_TypeExportConventionOverridden(type);
                    }
                    else
                    {
                        foreach (var iface in type.GetTypeInfo().ImplementedInterfaces)
                        {
                            if(iface == typeof(IDisposable))
                            {
                                continue;
                            }

                            // Run through the export specifications see if any match
                            foreach (var exportSpecification in this._interfaceExports)
                            {
                                if (exportSpecification.Item1 != null && exportSpecification.Item1(iface))
                                {
                                    ExportConventionBuilder exportBuilder = new ExportConventionBuilder();
                                    exportBuilder.AsContractType(iface);
                                    if (exportSpecification.Item2 != null)
                                    {
                                        exportSpecification.Item2(iface, exportBuilder);
                                    }
                                    exportBuilder.BuildAttributes(iface, ref attributes);
                                }
                            }
                        }
                    }
                    
                }
            }            
            return attributes;
        }

        internal bool BuildConstructorAttributes(Type type, ref List<Tuple<object, List<Attribute>>> configuredMembers)
        {
            IEnumerable<ConstructorInfo> constructors = type.GetTypeInfo().DeclaredConstructors;
            
            // First see if any of these constructors have the ImportingConstructorAttribute if so then we are already done
            foreach(var ci in constructors)
            {
                // We have a constructor configuration we must log a warning then not bother with ConstructorAttributes
                IEnumerable<Attribute> attributes = ci.GetCustomAttributes(typeof(ImportingConstructorAttribute), false);
                if(attributes.Count() != 0)
                {
                    CompositionTrace.Registration_ConstructorConventionOverridden(type);
                    return true;
                }
            }

            if (this._constructorFilter != null)
            {
                ConstructorInfo constructorInfo = this._constructorFilter(constructors);
                if(constructorInfo != null)
                {
                    ConfigureConstructorAttributes(constructorInfo, ref configuredMembers, this._configureConstuctorImports);
                }
                return true;
            }
            else if (this._configureConstuctorImports != null)
            {
                bool configured = false;
                foreach(var constructorInfo in FindLongestConstructors(constructors))
                {
                    ConfigureConstructorAttributes(constructorInfo, ref configuredMembers, this._configureConstuctorImports);
                    configured = true;
                }
                return configured;
            }
            return false;
        }

        internal static void BuildDefaultConstructorAttributes(Type type, ref List<Tuple<object, List<Attribute>>> configuredMembers)
        {
            IEnumerable<ConstructorInfo> constructors = type.GetTypeInfo().DeclaredConstructors;

            foreach(var constructorInfo in FindLongestConstructors(constructors))
            {
                ConfigureConstructorAttributes(constructorInfo, ref configuredMembers, null);
            }
        }

        private static void ConfigureConstructorAttributes(ConstructorInfo constructorInfo, ref List<Tuple<object, List<Attribute>>> configuredMembers, Action<ParameterInfo, ImportConventionBuilder> configureConstuctorImports)
        {
            if(configuredMembers == null)
            {
                configuredMembers = new List<Tuple<object, List<Attribute>>>();
            }

            // Make its attribute
            configuredMembers.Add(Tuple.Create((object)constructorInfo, ImportingConstructorList));

            //Okay we have the constructor now we can configure the ImportBuilders
            var parameterInfos = constructorInfo.GetParameters();
            foreach (var pi in parameterInfos)
            {
                bool isConfigured = pi.GetFirstAttribute<ImportAttribute>() != null || pi.GetFirstAttribute<ImportManyAttribute>() != null;
                if(isConfigured)
                {
                    CompositionTrace.Registration_ParameterImportConventionOverridden(pi, constructorInfo);
                }
                else
                {
                    var importBuilder = new ImportConventionBuilder();

                    // Let the developer alter them if they specified to do so
                    if (configureConstuctorImports != null)
                    {
                        configureConstuctorImports(pi, importBuilder);
                    }

                    // Generate the attributes
                    List<Attribute> attributes = null;
                    importBuilder.BuildAttributes(pi.ParameterType, ref attributes);
                    configuredMembers.Add(Tuple.Create((object)pi, attributes));
                }
            }
        }

        internal void BuildOnImportsSatisfiedNotification(Type type, ref List<Tuple<object, List<Attribute>>> configuredMembers)
        {
            //Add OnImportsSatisfiedAttribute where specified
            if (this._methodImportsSatisfiedNotifications != null)
            {
                foreach (var mi in type.GetRuntimeMethods())
                {
                    //We are only interested in void methods with no arguments
                    if(mi.ReturnParameter.ParameterType == typeof(void)
                     && mi.GetParameters().Length == 0)
                    {
                        MethodInfo underlyingMi = mi.DeclaringType.GetRuntimeMethod(mi.Name, _emptyTypeArray);
                        if (underlyingMi != null)
                        {
                            bool checkedIfConfigured = false;
                            bool isConfigured = false;
                            foreach (var notification in this._methodImportsSatisfiedNotifications)
                            {
                                if (notification(underlyingMi))
                                {
                                    if (!checkedIfConfigured)
                                    {
                                        isConfigured = mi.GetFirstAttribute<OnImportsSatisfiedAttribute>() != null;
                                        checkedIfConfigured = true;
                                    }

                                    if (isConfigured)
                                    {
                                        CompositionTrace.Registration_OnSatisfiedImportNotificationOverridden(type, mi);
                                        break;
                                    }
                                    else
                                    {
                                        // We really only need to create this list once and then cache it, it never goes back to null
                                        // Its perfectly okay if we make a list a few times on different threads, effectively though once we have 
                                        // cached one we will never make another.
                                        if (PartConventionBuilder._onImportsSatisfiedAttributeList == null)
                                        {
                                            var onImportsSatisfiedAttributeList = new List<Attribute>();
                                            onImportsSatisfiedAttributeList.Add(new OnImportsSatisfiedAttribute());
                                            PartConventionBuilder._onImportsSatisfiedAttributeList = onImportsSatisfiedAttributeList;
                                        }
                                        configuredMembers.Add(new Tuple<object, List<Attribute>>(mi, PartConventionBuilder._onImportsSatisfiedAttributeList));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void BuildPropertyAttributes(Type type, ref List<Tuple<object, List<Attribute>>> configuredMembers)
        {
            if(this._propertyImports.Any() || this._propertyExports.Any())
            {
                foreach (var pi in type.GetRuntimeProperties())
                {
                    List<Attribute> attributes = null;
                    int importsBuilt = 0;
                    bool checkedIfConfigured = false;
                    bool isConfigured = false;

                    PropertyInfo underlyingPi = null;

                    // Run through the import specifications see if any match
                    foreach(var importSpecification in this._propertyImports)
                    {
                        if (underlyingPi == null)
                        {
                            underlyingPi = pi.DeclaringType.GetRuntimeProperty(pi.Name);
                        }
                        if (importSpecification.Item1 != null && importSpecification.Item1(underlyingPi))
                        {
                            var importBuilder = new ImportConventionBuilder();

                            if(importSpecification.Item2 != null)
                            {
                                importSpecification.Item2(pi, importBuilder);
                            }

                            if(!checkedIfConfigured)
                            {
                                isConfigured = pi.GetFirstAttribute<ImportAttribute>() != null || pi.GetFirstAttribute<ImportManyAttribute>() != null;
                                checkedIfConfigured = true;
                            }

                            if(isConfigured)
                            {
                                CompositionTrace.Registration_MemberImportConventionOverridden(type, pi);
                                break;
                            }
                            else
                            {
                                importBuilder.BuildAttributes(pi.PropertyType, ref attributes);
                                ++importsBuilt;
                            }
                        }
                        if(importsBuilt > 1)
                        {
                            CompositionTrace.Registration_MemberImportConventionMatchedTwice(type, pi);
                        }
                    }
    
                    checkedIfConfigured = false;
                    isConfigured = false;

                    // Run through the export specifications see if any match
                    foreach(var exportSpecification in this._propertyExports)
                    {
                        if (underlyingPi == null)
                        {
                            underlyingPi = pi.DeclaringType.GetRuntimeProperty(pi.Name);
                        }

                        if (exportSpecification.Item1 != null && exportSpecification.Item1(underlyingPi))
                        {
                            var exportBuilder = new ExportConventionBuilder();

                            if (exportSpecification.Item3 != null)
                            {
                                exportBuilder.AsContractType(exportSpecification.Item3);
                            }

                            if(exportSpecification.Item2 != null)
                            {
                                exportSpecification.Item2(pi, exportBuilder);
                            }

                            if(!checkedIfConfigured)
                            {
                                isConfigured = pi.GetFirstAttribute<ExportAttribute>() != null || MemberHasExportMetadata(pi);
                                checkedIfConfigured = true;
                            }

                            if(isConfigured)
                            {
                                CompositionTrace.Registration_MemberExportConventionOverridden(type, pi);
                                break;
                            }
                            else
                            {
                                exportBuilder.BuildAttributes(pi.PropertyType, ref attributes);
                            }
                        }
                    }
    
                    if(attributes != null)
                    {
                        if(configuredMembers == null)
                        {
                            configuredMembers = new List<Tuple<object, List<Attribute>>>();
                        }
    
                        configuredMembers.Add(Tuple.Create((object)pi, attributes));
                    }
                }
            }
            return;
        }

        static IEnumerable<ConstructorInfo>FindLongestConstructors(IEnumerable<ConstructorInfo> constructors)
        {
            ConstructorInfo longestConstructor = null;
            int argumentsCount = 0;
            int constructorsFound = 0;

            foreach(var candidateConstructor in constructors)
            {
                int length = candidateConstructor.GetParameters().Length;
                if(length != 0)
                {
                    if(length > argumentsCount)
                    {
                        longestConstructor = candidateConstructor;
                        argumentsCount = length;
                        constructorsFound = 1;
                    }
                    else if(length == argumentsCount)
                    {
                        ++constructorsFound;
                    }
                }
            }
            if(constructorsFound > 1)
            {
                foreach(var candidateConstructor in constructors)
                {
                    int length = candidateConstructor.GetParameters().Length;
                    if(length == argumentsCount)
                    {
                        yield return candidateConstructor;
                    }
                }
            }
            else if(constructorsFound == 1)
            {
                yield return longestConstructor;
            }
            yield break;
        }
    }
}
