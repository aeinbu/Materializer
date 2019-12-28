using System;
using Xunit;
using Materializer;

namespace Materializer.Tests
{
	public class Materializer_Create
	{
		public Lazy<TypeGenerator> _lazy = new Lazy<TypeGenerator>(() => new TypeGenerator("Dynamic_Assembly_for_Materializer_Create_Tests", false));

		public interface IOne
		{
			int Prop1 { get; set; }
		}

		public interface ITwo
		{
			IOne Prop2 { get; set; }
		}

		public interface IThree : IOne
		{
			Single Prop3 { get; set; }
		}

		public interface IFour : IOne, ITwo
		{
		}


		[Fact]
		public void SimpleInterface()
		{
			var materializer = _lazy.Value;

			var one = materializer.New<IOne>();
			one.Prop1 = 3;

			Assert.Equal(3, one.Prop1);
		}

		[Fact]
		public void SimpleInterface_PropertyIsOtherSimpleInterface()
		{
			var materializer = _lazy.Value;

			var one = materializer.New<IOne>();

			var two = materializer.New<ITwo>();
			two.Prop2 = one;

			Assert.Equal(one, two.Prop2);
		}

		[Fact]
		public void InheritingInterface()
		{
			var materializer = _lazy.Value;

			var three = materializer.New<IThree>();
			three.Prop1 = 10;
			three.Prop3 = 2.2f;

			Assert.Equal(10, three.Prop1);
			Assert.Equal(2.2f, three.Prop3);
		}

		[Fact]
		public void CombiningInterface()
		{
			var materializer = _lazy.Value;
			var one = materializer.New<IOne>();

			var four = materializer.New<IFour>();
			four.Prop1 = 10;
			four.Prop2 = one;

			Assert.Equal(10, four.Prop1);
			Assert.Equal(one, four.Prop2);
		}

	}
}
