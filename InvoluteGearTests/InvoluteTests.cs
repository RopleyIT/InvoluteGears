using System;
using System.Collections.Generic;
using System.Drawing;
using Xunit;
using InvoluteGears;

namespace InvoluteGearTests
{
    public class InvoluteTests
    {
        [Fact]
        public void CrossOver()
        {
            List<PointF> list1 = new List<PointF>();
            list1.Add(new PointF(78, 30));
            list1.Add(new PointF(68, 31));
            list1.Add(new PointF(58, 32));
            list1.Add(new PointF(48, 33));
            list1.Add(new PointF(38, 34));
            list1.Add(new PointF(28, 35));
            list1.Add(new PointF(18, 36));
            list1.Add(new PointF(8,  37));
            List<PointF> list2 = new List<PointF>();
            list2.Add(new PointF(60.5f, 37));
            list2.Add(new PointF(59.5f, 36));
            list2.Add(new PointF(58.5f, 35));
            list2.Add(new PointF(57.5f, 34));
            list2.Add(new PointF(56.5f, 33));
            list2.Add(new PointF(55.5f, 32));
            list2.Add(new PointF(54.5f, 31));
            list2.Add(new PointF(53.5f, 30));

            var indices = Involutes.CrossOverIndices(list1, list2);
            Assert.Equal(1, indices.Index1);
            Assert.Equal(3, indices.Index2);
        }
    }
}
