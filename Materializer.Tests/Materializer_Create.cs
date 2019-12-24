using System;
using Xunit;
using Materializer;

namespace Materializer.Tests
{
	public class Materializer_Create
	{
		public Lazy<Materializer> _lazy = new Lazy<Materializer>(() => new Materializer(Guid.NewGuid().ToString()));

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

			var one = materializer.Create<IOne>();
			one.Prop1 = 3;

			Assert.Equal(3, one.Prop1);
		}

		[Fact]
		public void SimpleInterface_PropertyIsOtherSimpleInterface()
		{
			var materializer = _lazy.Value;

			var one = materializer.Create<IOne>();

			var two = materializer.Create<ITwo>();
			two.Prop2 = one;

			Assert.Equal(one, two.Prop2);
		}

		[Fact]
		public void InheritingInterface()
		{
			var materializer = _lazy.Value;

			var three = materializer.Create<IThree>();
			three.Prop1 = 10;
			three.Prop3 = 2.2f;

			Assert.Equal(10, three.Prop1);
			Assert.Equal(2.2f, three.Prop3);
		}

		[Fact]
		public void CombiningInterface()
		{
			var materializer = _lazy.Value;
			var one = materializer.Create<IOne>();

			var four = materializer.Create<IFour>();
			four.Prop1 = 10;
			four.Prop2 = one;

			Assert.Equal(10, four.Prop1);
			Assert.Equal(one, four.Prop2);
		}

	}
}
