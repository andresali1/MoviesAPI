namespace MoviesAPI.Tests.UnitTests
{
    [TestClass]
    public class ActorControllerTests : TestBase
    {
        /// <summary>
        /// Testing => Getting all actors with pagination params
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetAllWithPagination()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            context.Actor.Add(new Actor() { A_Name = "Actor 1" });
            context.Actor.Add(new Actor() { A_Name = "Actor 2" });
            context.Actor.Add(new Actor() { A_Name = "Actor 3" });

            await context.SaveChangesAsync();
            
            var context2 = BuildContext(bdName);

            // Test
            var controller = new ActorController(context2, mapper, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var page1 = await controller.Get(new PaginationDTO() { Page = 1, RecordsPerPage = 2 });

            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var page2 = await controller.Get(new PaginationDTO() { Page = 2, RecordsPerPage = 2 });

            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var page3 = await controller.Get(new PaginationDTO() { Page = 3, RecordsPerPage = 2 });

            // Verification
            var page1Actors = page1.Value;
            Assert.AreEqual(2, page1Actors.Count);

            var page2Actors = page2.Value;
            Assert.AreEqual(1, page2Actors.Count);

            var page3Actors = page3.Value;
            Assert.AreEqual(0, page3Actors.Count);
        }

        /// <summary>
        /// Testing => Get a 404 Status code by trying to get a non Existent Actor
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetByNonExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            // Test
            var controller = new ActorController(context, mapper, null);
            var response = await controller.Get(1);

            // Verification
            var result = response.Result as StatusCodeResult;
            Assert.AreEqual(404, result.StatusCode);
        }

        /// <summary>
        /// Testing => Get an Existent Actor by its Id
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetByExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            context.Actor.Add(new Actor() { A_Name = "Actor 1" });
            context.Actor.Add(new Actor() { A_Name = "Actor 2" });
            await context.SaveChangesAsync();

            var context2 = BuildContext(bdName);

            // Test
            var controller = new ActorController(context2, mapper, null);

            var id = 1;
            var response = await controller.Get(id);

            // Verification
            var result = response.Value;
            Assert.AreEqual(id, result.Id);
        }

        /// <summary>
        /// Testing => Creating a new Actor without Photo
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PostWithoutPhoto()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var actor = new ActorCreationDTO() { A_Name = "Actor 1", BirthDate = DateTime.Now };

            var mock = new Mock<IStoreFile>();
            mock.Setup(x => x.SaveFile(null, null, null, null))
                .Returns(Task.FromResult("url"));

            // Test
            var controller = new ActorController(context, mapper, mock.Object);
            var response = await controller.Post(actor);

            var context2 = BuildContext(bdName);

            // Verification
            var result = response as CreatedAtRouteResult;
            Assert.AreEqual(201, result.StatusCode);

            var list = await context2.Actor.ToListAsync();
            Assert.AreEqual(1, list.Count);
            Assert.IsNull(list[0].Photo);

            Assert.AreEqual(0, mock.Invocations.Count);
        }

        /// <summary>
        /// Testing => Creating a new Actor with Photo
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PostWithPhoto()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var content = Encoding.UTF8.GetBytes("Test Image");
            var file = new FormFile(new MemoryStream(content), 0, content.Length, "Data", "image.jpg");
            file.Headers = new HeaderDictionary();
            file.ContentType = "image/jpg";

            var actor = new ActorCreationDTO() { A_Name = "Actor 1", BirthDate = DateTime.Now, Photo = file };

            var mock = new Mock<IStoreFile>();
            mock.Setup(x => x.SaveFile(content, ".jpg", "actors", file.ContentType))
                .Returns(Task.FromResult("url"));

            var context2 = BuildContext(bdName);

            // Test
            var controller = new ActorController(context, mapper, mock.Object);
            var response = await controller.Post(actor);

            // Verification
            var result = response as CreatedAtRouteResult;
            Assert.AreEqual(201, result.StatusCode);

            var list = await context2.Actor.ToListAsync();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("url", list[0].Photo);

            Assert.AreEqual(1, mock.Invocations.Count);
        }

        /// <summary>
        /// Testing => Get 404 status code by trying to put a non Existent Actor
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutNonExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var mock = new Mock<IStoreFile>();
            mock.Setup(x => x.SaveFile(null, null, null, null))
                .Returns(Task.FromResult("url"));

            // Test
            var controller = new ActorController(context, mapper, mock.Object);
            var response = await controller.Put(1, new ActorCreationDTO() { A_Name = "Actor 1" });

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(404, result.StatusCode);
        }

        /// <summary>
        /// Testing => Update an Actor with put and without photo
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutWithNoPhoto()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            context.Actor.Add(new Actor() { A_Name = "Actor 1", BirthDate = DateTime.Now, Photo = "url" });
            await context.SaveChangesAsync();

            var actorCreationDTO = new ActorCreationDTO() { A_Name = "Modified", BirthDate = DateTime.Now.AddDays(-10) };

            var mock = new Mock<IStoreFile>();
            mock.Setup(x => x.SaveFile(null, null, null, null))
                .Returns(Task.FromResult("url"));

            var context2 = BuildContext(bdName);
            var context3 = BuildContext(bdName);

            // Test
            var controller = new ActorController(context2, mapper, mock.Object);

            var id = 1;
            var response = await controller.Put(id, actorCreationDTO);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(204, result.StatusCode);

            var actor = await context3.Actor.FirstOrDefaultAsync(a => a.Id == id);
            Assert.AreEqual(actorCreationDTO.A_Name, actor.A_Name);
            Assert.AreEqual(actorCreationDTO.BirthDate, actor.BirthDate);
            Assert.AreEqual("url", actor.Photo);

            Assert.AreEqual(0, mock.Invocations.Count);
        }

        /// <summary>
        /// Testing => Update an Actor with put and with photo
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutWithPhoto()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            context.Actor.Add(new Actor() { A_Name = "Actor 1", BirthDate = DateTime.Now });
            await context.SaveChangesAsync();

            var content = Encoding.UTF8.GetBytes("Test Image");
            var file = new FormFile(new MemoryStream(content), 0, content.Length, "Data", "image.jpg");
            file.Headers = new HeaderDictionary();
            file.ContentType = "image/jpg";

            var actorCreationDTO = new ActorCreationDTO() { A_Name = "Modified", BirthDate = DateTime.Now.AddMonths(-1), Photo = file };

            var mock = new Mock<IStoreFile>();
            mock.Setup(x => x.SaveFile(content, ".jpg", "actors", file.ContentType))
                .Returns(Task.FromResult("url"));

            // Test
            var context2 = BuildContext(bdName);
            var controller = new ActorController(context2, mapper, mock.Object);

            var id = 1;
            var response = await controller.Put(id, actorCreationDTO);

            var context3 = BuildContext(bdName);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(204, result.StatusCode);

            var actor = await context3.Actor.FirstOrDefaultAsync(a => a.Id == id);
            Assert.AreEqual(actorCreationDTO.A_Name, actor.A_Name);
            Assert.AreEqual(actorCreationDTO.BirthDate, actor.BirthDate);

            Assert.AreEqual(1, mock.Invocations.Count);
        }

        /// <summary>
        /// Testing => Get 404 Status code for trying to patch a non Existent Actor
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PatchNonExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var patchDocument = new JsonPatchDocument<ActorPatchDTO>();

            // Test
            var controller = new ActorController(context, mapper, null);
            var response = await controller.Patch(1, patchDocument);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(404, result.StatusCode);
        }

        /// <summary>
        /// Testing => Patch an Existent Actor
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PatchExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var birthDate = DateTime.Now;
            var savedActor = new Actor() { A_Name = "Actor 1", BirthDate = birthDate };
            context.Add(savedActor);
            await context.SaveChangesAsync();

            var newName = "Actor Modified";
            var patchDocument = new JsonPatchDocument<ActorPatchDTO>();
            patchDocument.Operations.Add(new Operation<ActorPatchDTO>("replace", "/a_Name", null, newName));

            var context2 = BuildContext(bdName);

            var objectValidator = new Mock<IObjectModelValidator>();
            objectValidator.Setup(x => x.Validate(
                It.IsAny<ActionContext>(),
                It.IsAny<ValidationStateDictionary>(),
                It.IsAny<string>(),
                It.IsAny<object>()
            ));

            var context3 = BuildContext(bdName);

            // Test
            var controller = new ActorController(context2, mapper, null);
            controller.ObjectValidator = objectValidator.Object;

            var response = await controller.Patch(1, patchDocument);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(204, result.StatusCode);

            var actorDb = await context3.Actor.FirstAsync();
            Assert.AreEqual(birthDate, actorDb.BirthDate);
            Assert.AreNotEqual(savedActor.A_Name, actorDb.A_Name);
            Assert.AreEqual(newName, actorDb.A_Name);
        }

        /// <summary>
        /// Testing => Get 404 by trying to delete a non Existent Actor
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DeleteNonExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            // Test
            var controller = new ActorController(context, mapper, null);
            var response = await controller.Delete(1);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(404, result.StatusCode);
        }

        /// <summary>
        /// Testing => Delete an Existent Actor
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DeleteExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var actor = new Actor() { A_Name = "Actor 1" };

            context.Actor.Add(actor);
            await context.SaveChangesAsync();

            var context2 = BuildContext(bdName);

            // Test
            var controller = new ActorController(context, mapper, null);
            var response = await controller.Delete(1);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(204, result.StatusCode);

            var exists = await context2.Actor.AnyAsync(a => a.A_Name.Equals(actor.A_Name));
            Assert.IsFalse(exists);
        }
    }
}