
using FluentAssertions;

using NUnit.Framework;

namespace ConfigHelper.Tests
{
    [TestFixture]
    public class WithPartialConfig
    {

        private class TestConfiguration
        {
            private readonly IConfigSettings configSettings;

            public TestConfiguration(IConfigSettings configSettings)
            {
                this.configSettings = configSettings;
            }

            public void Load()
            {
                configSettings.Load<PartialConfig>(this);
            }

            // ReSharper disable UnusedMember.Local
            [IsSetting]
            public string Item1 { get; set; }
            [IsSetting]
            public string Item2 { get; set; }
            [IsSetting]
            public int Item3 { get; set; }
            // ReSharper restore UnusedMember.Local
        }

        [Test]
        public void MissingSettingsHaveDefaultValue()
        {
            // Arrange
            var configSettings = new ConfigSettings();
            configSettings.RegisterConfigType(new PartialConfig());

            var config = new TestConfiguration(configSettings);

            // Act
            config.Load();

            // Assert
            config.Item2.Should().BeNull("there was no AppConfig value to load");
            config.Item3.Should().Be(0, "there was no AppConfig value to load");

        }
    }
}
