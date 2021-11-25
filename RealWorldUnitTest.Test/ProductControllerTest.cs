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
    public class ProductControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepository;
        private readonly ProductsController _controller;
        private List<Product> products;
        public ProductControllerTest()
        {
            _mockRepository = new Mock<IRepository<Product>>();
            _controller = new ProductsController(_mockRepository.Object);
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
        public async void Index_ActionExecutes_ReturnView()
        {
            var result = await _controller.Index();

            Assert.IsType<ViewResult>(result);
        }
        //3 urun kontrolu model 
        [Fact]
        public async void Index_ActionExecutes_ReturnProductList()
        {
            _mockRepository.Setup(repo => repo.GetAll()).ReturnsAsync(products);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);

            var productList = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);

            Assert.Equal<int>(3, productList.Count());
        }
        //RedirectToActionResult kontrolu
        [Fact]
        public async void Details_IdIsNull_ReturnRedirectToIndexAction()
        {
            var result = await _controller.Details(null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }
        //NotFoundResult kontrolu
        [Fact]
        public async void Details_IdInValid_ReturnNotFound()
        {
            Product product = null;
            _mockRepository.Setup(x => x.GetById(0)).ReturnsAsync(product);

            var result = await _controller.Details(0);

            var redirect = Assert.IsType<NotFoundResult>(result);

            Assert.Equal<int>(404, redirect.StatusCode);
        }
        //parametreli
        [Theory]
        [InlineData(1)]
        public async void Details_ValidId_ReturnProduct(int productId)
        {
            Product product = products.First(x => x.Id == productId);
            _mockRepository.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.Details(productId);

            var viewResult = Assert.IsType<ViewResult>(result);

            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }
        //Create get testi
        [Fact]
        public void Create_ActionExecutes_ReturnView()
        {
            var result = _controller.Create();
            Assert.IsType<ViewResult>(result);
        }
        //Crete Post
        [Fact]
        public async void CreatePost_InValideModelState_ReturnViews()
        {
            _controller.ModelState.AddModelError("Name", "Name alanı gereklidir.");

            var result = await _controller.Create(products.First());

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsType<Product>(viewResult.Model);

        }
        //Create Validator RedirectAction
        [Fact]
        public async void CreatePost_ValidModelState_ReturnRedirectoIndexAction()
        {
            var result = await _controller.Create(products.First());

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }
        //CreatePOST içerisinde en az 1 defa ilgili ürün çalışıyor mu kontrol edilmektedir.
        [Fact]
        public async void CreatePOST_ValidModelState_CreateMethodExecute()
        {
            Product newProduct = null;
            _mockRepository.Setup(x => x.Create(It.IsAny<Product>())).Callback<Product>(x => newProduct = x);

            var result = await _controller.Create(products.First());

            _mockRepository.Verify(x => x.Create(It.IsAny<Product>()), Times.Once);

            Assert.Equal(products.First().Id, newProduct.Id);
        }
        //model state Valid olmama durumu
        [Fact]
        public async void CreatePOST_InValidModelState_NeverCreateExecute()
        {
            _controller.ModelState.AddModelError("Name", "");

            var result = await _controller.Create(products.First());

            _mockRepository.Verify(x => x.Create(It.IsAny<Product>()),Times.Never);
        }
        //Edit redirectTo Action
        [Fact]
        public async void Edit_IdIsNUll_ReturnRedirectToAction()
        {
            var result = await _controller.Edit(null);
        
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            
            Assert.Equal("Index", redirect.ActionName);
        }

        //Edit Return Not Found
        [Theory]
        [InlineData(3)]
        public async void Edit_IdInValid_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepository.Setup(x => x.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.Edit(productId);

            var redirect = Assert.IsType<NotFoundResult>(result);

            Assert.Equal<int>(404, redirect.StatusCode);
        }
        //Id olan Product test
        [Theory]
        [InlineData(3)]
        public async void Edit_ActionExecute_ReturnProduct(int productId)
        {
            var product = products.First(x => x.Id == productId);
            _mockRepository.Setup(x => x.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.Edit(productId);

            var ViewResult = Assert.IsType<ViewResult>(result);
            //object ile miras alıyor mu
            var resultProduct = Assert.IsAssignableFrom<Product>(ViewResult.Model);

            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }
        //Id si 1 olan verip 2 olan farklı olan bekleniyor NotFound testi
        [Theory]
        [InlineData(1)]
        public void EditPOST_IdIsNotFoundEqualProduct_ReturnNotFound(int productId)
        {
            var result = _controller.Edit(2, products.First(x => x.Id == productId));

            var redirect = Assert.IsType<NotFoundResult>(result);
        }
        //ID si 1 olan gelen data dogrumu
        [Theory]
        [InlineData(1)]
        public void EditPost_InValidModelState_ReturnView(int productId)
        {
            _controller.ModelState.AddModelError("Name","");
            //gelen Id eşleşmesi
            var result = _controller.Edit(productId, products.First(x => x.Id == productId));

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(viewResult.Model);
        }
        //Edit POST ID si 1 olan gelen data  redirecToAction
        [Theory]
        [InlineData(1)]
        public void EditPost_InValidModelState_ReturRedirecToAction(int productId)
        {
            var result = _controller.Edit(productId, products.First(x => x.Id == productId));

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        //Edit POST Model state IsValid oldugu zaman Update test
        [Theory]
        [InlineData(1)]
        public void EditPost_InValidModelState_UpdateMethodExecute(int productId)
        {
            var product = products.First(x => x.Id == productId);
            _mockRepository.Setup(repo => repo.Update(product));

            _controller.Edit(productId, product);

            _mockRepository.Verify(x => x.Update(It.IsAny<Product>()), Times.Once);
        }
    }
}
