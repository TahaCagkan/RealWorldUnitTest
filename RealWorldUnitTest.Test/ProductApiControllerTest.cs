using Microsoft.AspNetCore.Mvc;
using Moq;
using RealWorldUnitTest.Web.Controllers;
using RealWorldUnitTest.Web.Models;
using RealWorldUnitTest.Web.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RealWorldUnitTest.Test
{
    public class ProductApiControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsApiController _controller;

        private List<Product> products;

        public ProductApiControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductsApiController(_mockRepo.Object);

            products = new List<Product>() { new Product {
                Id = 1,
                Name = "Kalem",
                Price = 100,
                Stock = 250,
                Color="Mavi"
            },
            new Product {
                Id = 2,
                Name = "Çanta",
                Price = 200,
                Stock = 180,
                Color="Bordo"
            },
               new Product {
                Id = 3,
                Name = "Silgi",
                Price = 75,
                Stock = 140,
                Color="Siyah"
            }
            };
        }
        [Fact]
        public async void GetProduct_ActionExecutes_ReturnOkResultWithProduct()
        {
            _mockRepo.Setup(x => x.GetAll()).ReturnsAsync(products);

            var result = await _controller.GetProduct();
            var okResult = Assert.IsType<OkObjectResult>(result);
            //donen result Value su alinmaktadir.
            var returnProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);

            Assert.Equal<int>(3, returnProducts.ToList().Count);
        }

        [Theory]
        [InlineData(1)]
        public void PutProduct_IdIsNotEqualProduct_ReturnBadRequestResult(int productId)
        {
            var product = products.First(x => x.Id == productId);
            var result = _controller.PutProduct(3, product);

            var badRequestResult = Assert.IsType<BadRequestResult>(result);
        }
    }
}
