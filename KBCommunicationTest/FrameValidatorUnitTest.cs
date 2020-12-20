using KBCommunication;
using System;
using Xunit;

namespace KBCommunicationTest
{
    public class FrameValidatorUnitTest
    {
        [Fact]
        public void TestFrameValidated()
        {
            // Arrange
            var eventFlag = false;
            var validator = new FrameValidator<bool>(x => x);
            validator.FrameValidated += (s, e) => eventFlag = e;

            // Act
            validator.RegisterData(true);

            // Assert
            Assert.True(eventFlag);
        }

        [Fact]
        public void TestValidationFailed()
        {
            // Arrange
            var eventFlag = true;
            var validator = new FrameValidator<bool>(x => x);
            validator.ValidationFailed += (s, e) => eventFlag = e;

            // Act
            validator.RegisterData(false);

            // Assert
            Assert.False(eventFlag);
        }

        [Fact]
        public void TestRegisterData()
        {
            // Arrange
            object testObject = new object();
            object testObject2 = new object();
            object validatedObject = null;
            object failedObject = null;
            var validator = new FrameValidator<object>(x => x.Equals(testObject));
            validator.FrameValidated += (s, e) => validatedObject = e;
            validator.ValidationFailed += (s, e) => failedObject = e;

            // Act
            validator.RegisterData(testObject);
            validator.RegisterData(testObject2);

            // Assert
            Assert.Equal(testObject, validatedObject);
            Assert.Equal(testObject2, failedObject);
        }

        [Fact]
        public void TestFromFrame()
        {
            // Arrange
            var testObject = new object();
            var validator = new FrameValidator<object>(x => true);

            // Act
            var frame = validator.FromFrame(testObject);

            // Assert
            Assert.Equal(testObject, frame);
        }

        [Fact]
        public void TestValidate()
        {
            // Arrange
            var result = true;
            bool validateFunc(bool x) => result;
            var validator = new FrameValidator<bool>(validateFunc);

            // Act
            var testTrueTrue = validator.Validate(true);
            var testFalseTrue = validator.Validate(false);
            result = false;
            var testTrueFalse = validator.Validate(true);
            var testFalseFalse = validator.Validate(false);

            // Assert
            Assert.True(testTrueTrue);
            Assert.True(testFalseTrue);
            Assert.False(testTrueFalse);
            Assert.False(testFalseFalse);
        }
    }
}
