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
	public class Materializer_Serializable
	{
		public Lazy<TypeGenerator> _lazy = new Lazy<TypeGenerator>(() => new TypeGenerator("Dynamic_Assembly_for_Materializer_Serializable_Tests", true));

		public interface IOne
		{
			int Prop1 { get; set; }
		}


		[Fact]
		public void SimpleInterface_BinaryFormatter()
		{
			var materializer = _lazy.Value;

			var before = materializer.New<IOne>();
			before.Prop1 = 3;

			IFormatter formatter = new BinaryFormatter();
			using var stream = new MemoryStream();
			formatter.Serialize(stream, before);

			stream.Seek(0, SeekOrigin.Begin);
			var after = (IOne)formatter.Deserialize(stream);

			Assert.Equal(3, after.Prop1);
		}

	}
}
