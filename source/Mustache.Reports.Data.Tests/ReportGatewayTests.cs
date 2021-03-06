﻿using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;
using Microsoft.Extensions.Options;
using Mustache.Reports.Boundary.Options;
using Mustache.Reports.Boundary.Report.Excel;
using Mustache.Reports.Boundary.Report.Word;
using NSubstitute;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mustache.Reports.Data.Tests
{
    public class ReportGatewayTests
    {
        [Fact]
        public void CreateWordReport_WhenValidInput_ShouldReturnRenderedReport()
        {
            //---------------Arrange------------------
            var configuration = SetupConfiguration();
            var reportData = File.ReadAllText("ExampleData\\WithImagesSampleData.json");
            var wordGateway = new ReportGateway(configuration);
            var input = new RenderWordInput {JsonModel = reportData, ReportName = "test.docx", TemplateName = "ReportWithImages" };
            //---------------Act----------------------
            var actual = wordGateway.CreateWordReport(input);
            //---------------Assert-------------------
            var expected = File.ReadAllText("Expected\\RenderedWordBase64.txt");
            Assert.Equal(expected.Substring(0,50), actual.Base64String.Substring(0,50));
        }

        [Fact]
        public void CreateExcelReport_WhenValidInput_ShouldReturnRenderedReport()
        {
            //---------------Arrange------------------
            var configuration = SetupConfiguration();
            var reportData = File.ReadAllText("ExampleData\\ExcelSampleData.json");
            var wordGateway = new ReportGateway(configuration);
            var input = new RenderExcelInput { JsonModel = reportData, ReportName = "test.xslx", TemplateName = "SimpleReport" };
            //---------------Act----------------------
            var actual = wordGateway.CreateExcelReport(input);
            //---------------Assert-------------------
            var expected = File.ReadAllText("Expected\\RenderedExcelBase64.txt");
            Assert.Equal(expected.Substring(0, 50), actual.Base64String.Substring(0, 50));
        }

        [Fact]
        public void CreateExcelReport_WhenInvalidTemplateName_ShouldReturnTemplateNameError()
        {
            //---------------Arrange------------------
            var configuration = SetupConfiguration();
            var reportGateway = new ReportGateway(configuration);
            var input = new RenderExcelInput { JsonModel = "", ReportName = "test.xslx", TemplateName = "INVALID_NAME" };
            //---------------Act----------------------
            var actual = reportGateway.CreateExcelReport(input);
            //---------------Assert-------------------
            Assert.True(actual.HasErrors());
            Assert.Contains("Invalid Report Template", actual.ErrorMessages[0]);
            Assert.Contains("INVALID_NAME.xlsx", actual.ErrorMessages[0]);
        }

        [Fact]
        public void CreateExcelReport_GivenTemplate_Has_Dynamic_Chart_ShouldReturnRenderedReport()
        {
            //---------------Arrange------------------
            var configuration = SetupConfiguration();
            var reportData = File.ReadAllText("ExampleData\\dynamic-chart-range.json");
            var wordGateway = new ReportGateway(configuration);
            var input = new RenderExcelInput
            {
                JsonModel = reportData,
                ReportName = "test.xslx",
                TemplateName = "dynamic-chart",
                SheetNumber = 1
            };
            //---------------Act----------------------
            var actual = wordGateway.CreateExcelReport(input);
            //---------------Assert-------------------
            var expected = File.ReadAllText("Expected\\RenderedDynamicChartExcelBase64.txt");
            Assert.Equal(expected.Substring(0, 50), actual.Base64String.Substring(0, 50));
        }

        [Fact]
        public void CreateExcelReport_When_Multiple_Sheets_Expect_Both_Populated()
        {
            //---------------Arrange------------------
            var configuration = SetupConfiguration();
            var reportData = File.ReadAllText("ExampleData\\CampaignDetailedReport.json");
            var wordGateway = new ReportGateway(configuration);
            
            var input = new RenderExcelInput
            {
                JsonModel = reportData,
                ReportName = "test.xslx",
                TemplateName = "CampaignDetailedReport",
                SheetNumbers = new List<int> { 1, 2 }
            };
            //---------------Act----------------------
            var actual = wordGateway.CreateExcelReport(input);
            //---------------Assert-------------------
            File.WriteAllBytes("c:\\tmp\\two-tab.xlsx",actual.FetchDocumentAsByteArray());
            var expected = File.ReadAllText("Expected\\RenderedCampaignDetailedReportBase64.txt");
            Assert.Equal(expected.Substring(0, 50), actual.Base64String.Substring(0, 50));
        }

        private static IOptions<MustacheReportOptions> SetupConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();
            var data = GetAppSettingJsonData();

            // ustacheReportOptions
            var reportOptions = JsonConvert.DeserializeObject<MustacheReportOptionsWrapper>(data);

            var result = Substitute.For<IOptions<MustacheReportOptions>>();
            result.Value.Returns(reportOptions.MustacheReportOptions);

            return result;
        }

        private static string GetAppSettingJsonData()
        {
            var filePath = Directory.GetCurrentDirectory();
            var settingsPath = Path.Combine(filePath, "appsettings.json");
            var data = File.ReadAllText(settingsPath);
            return data;
        }

        public class MustacheReportOptionsWrapper
        {
            public MustacheReportOptions MustacheReportOptions { get; set; }
        }
    }
}
