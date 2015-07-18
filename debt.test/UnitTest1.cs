using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using debt_fe.Controllers;
using debt_fe.Models.ViewModels;
using System.Web.Mvc;
using debt_fe.Models;

namespace debt.test
{
	[TestClass]
	public class DocumentTest
	{
		[TestMethod]
		public void TestUploadDocument()
		{
            var controller = new DocumentController();
            var viewModel = new DocumentViewModel();

            // viewModel.UploadedFile = new HttpPostedFileBase

            // var result = controller.UploadDocument(viewModel) as ViewResult;

            // var context = new Mock<HttpContextBase>();

            

            // Assert.IsNotNull((result.Model as DocumentModel).FileName);
		}
	}
}
