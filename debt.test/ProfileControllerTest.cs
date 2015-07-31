using debt_fe.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace debt.test
{
    [TestClass]
   public class ProfileControllerTest
    {
        private T GetValueFromJsonResult<T>(JsonResult jsonResult, string propertyName)
        {
            var property =
                jsonResult.Data.GetType().GetProperties()
                .Where(p => string.Compare(p.Name, propertyName) == 0)
                .FirstOrDefault();

            if (null == property)
                throw new ArgumentException("propertyName not found", "propertyName");
            return (T)property.GetValue(jsonResult.Data, null);
        }

        [TestMethod]
        public void TestLoadUpdateHistoryXML()
        {
            var controller = new ProfileController();

            var result = controller.LoadUpdateHistory() as JsonResult;

            var json = GetValueFromJsonResult<int>(result, "code");

            Assert.AreEqual(json, 1);

            // Assert.IsTrue((dynamic)result.Data.code == 1);
        }
    }
}
