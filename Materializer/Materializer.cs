using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Materializer
{
	public class Materializer
	{
		private readonly ModuleBuilder _moduleBuilder;
		private readonly AssemblyBuilder _dynamicAssembly;
		private readonly bool _forSerializableTypes;
		private readonly Dictionary<Type, Type> _typeCache = new Dictionary<Type, Type>();

		public Materializer(string assemblyName = "Dynamic_assembly_for_Materializer_created_types", bool forSerializable = false)
		{
			_dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
			_moduleBuilder = _dynamicAssembly.DefineDynamicModule(assemblyName);
			_forSerializableTypes = forSerializable;

			if (_forSerializableTypes)
			{
				var currentAppDomain = AppDomain.CurrentDomain;
				currentAppDomain.AssemblyResolve += new ResolveEventHandler(DynamicAssemblyResolvehandler);
			}
		}


		private Assembly DynamicAssemblyResolvehandler(object sender, ResolveEventArgs args)
		{
			if (args.Name == _dynamicAssembly.FullName)
			{
				return _dynamicAssembly;
			}

			return null;
		}


		public T New<T>() where T : class
		{
			var t = GetOrCreateType<T>();
			var instance = Activator.CreateInstance(t);
			return (T)instance;
		}


		public Type ConcreteTypeOf<T>() where T : class
		{
			return GetOrCreateType<T>();
		}


		private Type GetOrCreateType<T>() where T : class
		{
			var interfaceOfTypeToCreate = typeof(T);
			if (!interfaceOfTypeToCreate.IsInterface)
			{
				throw new InvalidOperationException("Can only create objects for interface types");
			}

			if (!_typeCache.ContainsKey(interfaceOfTypeToCreate))
			{
				var createdType = CreateType<T>(interfaceOfTypeToCreate);
				_typeCache.Add(interfaceOfTypeToCreate, createdType);
			}

			return _typeCache[interfaceOfTypeToCreate];
		}


		private Type CreateType<T>(Type interfaceOfTypeToCreate) where T : class
		{
			if (_typeCache.ContainsKey(interfaceOfTypeToCreate))
			{
				return _typeCache[interfaceOfTypeToCreate];
			}

			var typename = $"{interfaceOfTypeToCreate.Name}_{Guid.NewGuid()}";
			var typeBuilder = _moduleBuilder.DefineType(typename, TypeAttributes.Public);

			if (_forSerializableTypes)
			{
				var serializableAttributeTypeInfo = typeof(SerializableAttribute);
				var serializableAttributeConstructorInfo = serializableAttributeTypeInfo.GetConstructor(new Type[] { });
				var serializableAttributeBuilder = new CustomAttributeBuilder(serializableAttributeConstructorInfo, new object[] { });
				typeBuilder.SetCustomAttribute(serializableAttributeBuilder);
			}

			ImplementInterfaceProperties(interfaceOfTypeToCreate, typeBuilder);

			return typeBuilder.CreateType();
		}


		private void ImplementInterfaceProperties(Type interfaceOfTypeToCreate, TypeBuilder typeBuilder)
		{
			typeBuilder.AddInterfaceImplementation(interfaceOfTypeToCreate);
			foreach (var implementedInterfaceType in interfaceOfTypeToCreate.GetInterfaces())
			{
				ImplementInterfaceProperties(implementedInterfaceType, typeBuilder);
			}

			foreach (var pi in interfaceOfTypeToCreate.GetProperties())
			{
				var backingFieldBuilder = typeBuilder.DefineField($"{interfaceOfTypeToCreate.Name}._{ToCamelCase(pi.Name)}", pi.PropertyType, FieldAttributes.Private);

				var accessorAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.HideBySig;

				var mbGetAccessor = typeBuilder.DefineMethod($"get_{pi.Name}", accessorAttributes, pi.PropertyType, Type.EmptyTypes);
				var mbGetIL = mbGetAccessor.GetILGenerator();
				mbGetIL.Emit(OpCodes.Ldarg_0);
				mbGetIL.Emit(OpCodes.Ldfld, backingFieldBuilder);
				mbGetIL.Emit(OpCodes.Ret);

				var mbSetAccessor = typeBuilder.DefineMethod($"set_{pi.Name}", accessorAttributes, null, new[] { pi.PropertyType });
				var mbSetIL = mbSetAccessor.GetILGenerator();
				mbSetIL.Emit(OpCodes.Ldarg_0);
				mbSetIL.Emit(OpCodes.Ldarg_1);
				mbSetIL.Emit(OpCodes.Stfld, backingFieldBuilder);
				mbSetIL.Emit(OpCodes.Ret);

				var propertyBuilder = typeBuilder.DefineProperty(pi.Name, PropertyAttributes.HasDefault, pi.PropertyType, null);
				propertyBuilder.SetGetMethod(mbGetAccessor);
				propertyBuilder.SetSetMethod(mbSetAccessor);
			}
		}


		private string ToCamelCase(string s)
		{
			var arr = s.ToCharArray();
			arr[0] = Char.ToLowerInvariant(arr[0]);
			return new string(arr);
		}
	}
}