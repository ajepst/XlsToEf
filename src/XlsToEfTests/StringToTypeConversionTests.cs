using System;
using Shouldly;
using XlsToEf.Import;

namespace XlsToEfTests
{
    public class StringToTypeConversionTests
    {
        public void Should_Convert_Guids()
        {
            var guidType = typeof (Guid);
            var refGuid = new Guid("00000000000000000000000000000000");
            var guid1 = StringToTypeConverter.Convert("00000000000000000000000000000000", guidType );
            var guid2 = StringToTypeConverter.Convert("00000000-0000-0000-0000-000000000000", guidType);
            var guid3 = StringToTypeConverter.Convert("{00000000-0000-0000-0000-000000000000}", guidType);
            var guid4 = StringToTypeConverter.Convert("(00000000-0000-0000-0000-000000000000)", guidType);
            var guid5 = StringToTypeConverter.Convert("{0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}}", guidType);

            guid1.ShouldBe(refGuid);
            guid2.ShouldBe(refGuid);
            guid3.ShouldBe(refGuid);
            guid4.ShouldBe(refGuid);
            guid5.ShouldBe(refGuid);
        }

        public void Should_Fail_Converting_Null_Guid_on_Non_Nullable()
        {
            Should.Throw<Exception>(() => StringToTypeConverter.Convert(null, typeof (Guid)));
        }

        public void Should_Return_Null_When_Converting_Null_Guid_For_Nullable()
        {
            var guid = StringToTypeConverter.Convert(null, typeof(Guid?));
            guid.ShouldBe(null);
        }
   


        public void Should_Not_Convert_Strings()
        {
            const string refStr = "A string";
            var converted = StringToTypeConverter.Convert(refStr, typeof (string));
            converted.ShouldBe(refStr);
        }

        public void Should_Convert_Integers()
        {
            const int number = 1000000;
            var converted1 = StringToTypeConverter.Convert("1000000", typeof(int));
            var converted2 = StringToTypeConverter.Convert("1,000,000", typeof(int));
            converted1.ShouldBe(number);
            converted2.ShouldBe(number);
        } 
    }
}