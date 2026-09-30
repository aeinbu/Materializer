using System;
using Xunit;
using Materializer;

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
		public void SimpleInterface_HasSerializableAttribute()
		{
			var materializer = _lazy.Value;

			var generatedType = materializer.ConcreteTypeOf<IOne>();

			Assert.True(Attribute.IsDefined(generatedType, typeof(SerializableAttribute)));
		}

	}
}
