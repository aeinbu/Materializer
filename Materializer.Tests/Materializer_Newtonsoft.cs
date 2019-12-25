using System;
using System.IO;
using System.Runtime.Serialization;
using Xunit;
using Materializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace Materializer.Tests
{
	public class Materializer_Newtonsoft
	{
		public Lazy<Materializer> _lazy = new Lazy<Materializer>(() => new Materializer("Dynamic_Assembly_for_Materializer_Newtonsoft_Tests", false));

		public interface IOne
		{
			int Prop1 { get; set; }
		}


		[Fact]
		public void SimpleInterface_NewtonsSoft_JsonSerialize()
		{
			var materializer = _lazy.Value;

			var before = materializer.New<IOne>();
			before.Prop1 = 3;
			var json = JsonConvert.SerializeObject(before);

			var after = (IOne)JsonConvert.DeserializeObject(json, materializer.ConcreteTypeOf<IOne>());

			Assert.Equal(3, after.Prop1);
		}


		[Fact]
		public void SimpleInterface_NewtonsSoft_JsonSerialize_AlternativeSample()
		{
			var materializer = _lazy.Value;
			T DeserializeInterface<T>(string json) where T : class
			{
				return (T)JsonConvert.DeserializeObject(json, materializer.ConcreteTypeOf<T>());
			}

			var before = materializer.New<IOne>();
			before.Prop1 = 3;
			var json = JsonConvert.SerializeObject(before);

			var after = DeserializeInterface<IOne>(json);

			Assert.Equal(3, after.Prop1);
		}

	}
}
