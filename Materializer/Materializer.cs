using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Materializer
{
	public class Materializer
	{
		private readonly ModuleBuilder _moduleBuilder;
		private readonly Dictionary<Type, Type> _typeCache = new Dictionary<Type, Type>();

		public Materializer(string assemblyName)
		{
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
			_moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);
		}

		public T Create<T>(bool markAsSerializable = false) where T : class
		{
			var interfaceOfTypeToCreate = typeof(T);
			if (!interfaceOfTypeToCreate.IsInterface)
			{
				throw new InvalidOperationException("Can only create objects for interface types");
			}

			if (!_typeCache.ContainsKey(interfaceOfTypeToCreate))
			{
				var createdType = CreateClass<T>(markAsSerializable, interfaceOfTypeToCreate);
				_typeCache.Add(interfaceOfTypeToCreate, createdType);
			}

			var t = _typeCache[interfaceOfTypeToCreate];
			var instance = Activator.CreateInstance(t);
			return (T)instance;
		}

		private Type CreateClass<T>(bool markAsSerializable, Type interfaceOfTypeToCreate) where T : class
		{
			var typename = interfaceOfTypeToCreate.Name + Guid.NewGuid();
			var typeBuilder = _moduleBuilder.DefineType(typename, TypeAttributes.Public);

			if (markAsSerializable)
			{
				typeBuilder.SetCustomAttribute(typeof(SerializableAttribute).GetConstructor(new Type[]{}), null);
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

//TODO: how to make seperate get_interfacename.propertyname for different interfaces?
				var mbGetAccessor = typeBuilder.DefineMethod($"get_{pi.Name}", accessorAttributes, pi.PropertyType, Type.EmptyTypes);
				var mbGetIL = mbGetAccessor.GetILGenerator();
				mbGetIL.Emit(OpCodes.Ldarg_0);
				mbGetIL.Emit(OpCodes.Ldfld, backingFieldBuilder);
				mbGetIL.Emit(OpCodes.Ret);

				var mbSetAccessor = typeBuilder.DefineMethod($"set_{pi.Name}", accessorAttributes, null, new []{ pi.PropertyType });
				var mbSetIL = mbSetAccessor.GetILGenerator();
				mbSetIL.Emit(OpCodes.Ldarg_0);
				mbSetIL.Emit(OpCodes.Ldarg_1);
				mbSetIL.Emit(OpCodes.Stfld, backingFieldBuilder);
				mbSetIL.Emit(OpCodes.Ret);

				var propertyBuilder = typeBuilder.DefineProperty($"{interfaceOfTypeToCreate.Name}.{pi.Name}", PropertyAttributes.HasDefault, pi.PropertyType, null);
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