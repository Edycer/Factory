﻿using Core;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using Xunit;

namespace Utils.Tests
{
    public class SingletonManagerTests
    {
        Mock<ILogger> mockLogger;
        Mock<IFactory> mockFactory;
        Mock<IReflection> mockReflection;
        SingletonManager testObject;

        void Init()
        {
            mockFactory = new Mock<IFactory>();
            mockReflection = new Mock<IReflection>();
            mockLogger = new Mock<ILogger>();
            testObject = new SingletonManager(mockFactory.Object, mockReflection.Object, mockLogger.Object);
        }

        [Fact]
        public void init_singletons_when_there_are_no_singletons()
        {
            Init();

            testObject.InstantiateSingletons();

            Assert.Empty(testObject.Singletons);
        }

        [Fact]
        public void can_find_singleton()
        {
            Init();

            var mockType = typeof(int);
            var mockTypes = new Type[] { mockType };
            var mockSingleton = new object();

            mockReflection
                .Setup(m => m.FindTypesMarkedByAttributes(new Type[] { typeof(SingletonAttribute) }))
                .Returns(mockTypes);

            mockReflection
                .Setup(m => m.GetAttributes<SingletonAttribute>(mockType))
                .Returns(new SingletonAttribute[] { new SingletonAttribute() });

            mockFactory
                .Setup(m => m.Create(mockType))
                .Returns(mockSingleton);
            mockFactory
                .Setup(m => m.OrderByDeps<SingletonAttribute>(mockTypes, It.IsAny<Func<SingletonAttribute, string>>()))
                .Returns(mockTypes);

            testObject.InstantiateSingletons();

            Assert.Equal(1, testObject.Singletons.Length);
            Assert.Equal(mockSingleton, testObject.Singletons[0]);
        }

        [Fact]
        public void can_register_singleton_as_dependency()
        {
            Init();

            var mockType = typeof(int);
            var mockTypes = new Type[] { mockType };
            var mockSingleton = new object();

            mockReflection
                .Setup(m => m.FindTypesMarkedByAttributes(new Type[] { typeof(SingletonAttribute) }))
                .Returns(mockTypes);

            mockReflection
                .Setup(m => m.GetAttributes<SingletonAttribute>(mockType))
                .Returns(new SingletonAttribute[] { new SingletonAttribute(mockType) });

            mockFactory
                .Setup(m => m.Create(mockType))
                .Returns(mockSingleton);
            mockFactory
                .Setup(m => m.OrderByDeps<SingletonAttribute>(mockTypes, It.IsAny<Func<SingletonAttribute, string>>()))
                .Returns(mockTypes);

            testObject.InstantiateSingletons();

            mockFactory.Verify(m => m.Dep(mockType, mockSingleton), Times.Once());
        }

        [Fact]
        public void non_startable_singletons_are_ignored_on_start()
        {
            Init();

            var mockType = typeof(int);
            var mockTypes = new Type[] { mockType };
            var mockNonStartableSingleton = new object();

            mockReflection
                .Setup(m => m.FindTypesMarkedByAttributes(new Type[] { typeof(SingletonAttribute) }))
                .Returns(mockTypes);

            mockReflection
                .Setup(m => m.GetAttributes<SingletonAttribute>(mockType))
                .Returns(new SingletonAttribute[] { new SingletonAttribute(mockType) });

            mockFactory
                .Setup(m => m.Create(mockType))
                .Returns(mockNonStartableSingleton);
            mockFactory
                .Setup(m => m.OrderByDeps<SingletonAttribute>(mockTypes, It.IsAny<Func<SingletonAttribute, string>>()))
                .Returns(mockTypes);

            testObject.InstantiateSingletons();

            testObject.Start();
        }

        [Fact]
        public void non_startable_singletons_are_ignored_on_shutdown()
        {
            Init();

            var mockType = typeof(int);
            var mockTypes = new Type[] { mockType };
            var mockNonStartableSingleton = new object();

            mockReflection
                .Setup(m => m.FindTypesMarkedByAttributes(new Type[] { typeof(SingletonAttribute) }))
                .Returns(mockTypes);

            mockReflection
                .Setup(m => m.GetAttributes<SingletonAttribute>(mockType))
                .Returns(new SingletonAttribute[] { new SingletonAttribute(mockType) });

            mockFactory
                .Setup(m => m.Create(mockType))
                .Returns(mockNonStartableSingleton);
            mockFactory
                .Setup(m => m.OrderByDeps<SingletonAttribute>(mockTypes, It.IsAny<Func<SingletonAttribute, string>>()))
                .Returns(mockTypes);

            testObject.InstantiateSingletons();

            testObject.Shutdown();
        }

        [Fact]
        public void can_start_singletons()
        {
            Init();

            var mockType = typeof(float);
            var mockTypes = new Type[] { mockType };
            var mockStartableSingleton = new Mock<IStartable>();

            mockReflection
                .Setup(m => m.FindTypesMarkedByAttributes(new Type[] { typeof(SingletonAttribute) }))
                .Returns(mockTypes);

            mockReflection
                .Setup(m => m.GetAttributes<SingletonAttribute>(mockType))
                .Returns(new SingletonAttribute[] { new SingletonAttribute(mockType) });

            mockFactory
                .Setup(m => m.Create(mockType))
                .Returns(mockStartableSingleton.Object);
            mockFactory
                .Setup(m => m.OrderByDeps<SingletonAttribute>(mockTypes, It.IsAny<Func<SingletonAttribute, string>>()))
                .Returns(mockTypes);

            testObject.InstantiateSingletons();

            testObject.Start();

            mockStartableSingleton.Verify(m => m.Start(), Times.Once());
        }

        [Fact]
        public void can_shutdown_singletons()
        {
            Init();

            var mockType = typeof(float);
            var mockTypes = new Type[] { mockType };
            var mockStartableSingleton = new Mock<IStartable>();

            mockReflection
                .Setup(m => m.FindTypesMarkedByAttributes(new Type[] { typeof(SingletonAttribute) }))
                .Returns(mockTypes);

            mockReflection
                .Setup(m => m.GetAttributes<SingletonAttribute>(mockType))
                .Returns(new SingletonAttribute[] { new SingletonAttribute(mockType) });

            mockFactory
                .Setup(m => m.Create(mockType))
                .Returns(mockStartableSingleton.Object);
            mockFactory
                .Setup(m => m.OrderByDeps<SingletonAttribute>(mockTypes, It.IsAny<Func<SingletonAttribute, string>>()))
                .Returns(mockTypes);

            testObject.InstantiateSingletons();

            testObject.Shutdown();

            mockStartableSingleton.Verify(m => m.Shutdown(), Times.Once());
        }

        [Fact]
        public void start_exceptions_are_swallowed_and_logged()
        {
            Init();

            var mockType = typeof(float);
            var mockTypes = new Type[] { mockType };
            var mockStartableSingleton = new Mock<IStartable>();

            mockReflection
                .Setup(m => m.FindTypesMarkedByAttributes(new Type[] { typeof(SingletonAttribute) }))
                .Returns(mockTypes);

            mockReflection
                .Setup(m => m.GetAttributes<SingletonAttribute>(mockType))
                .Returns(new SingletonAttribute[] { new SingletonAttribute(mockType) });

            mockFactory
                .Setup(m => m.Create(mockType))
                .Returns(mockStartableSingleton.Object);
            mockFactory
                .Setup(m => m.OrderByDeps<SingletonAttribute>(mockTypes, It.IsAny<Func<SingletonAttribute, string>>()))
                .Returns(mockTypes);

            testObject.InstantiateSingletons();

            mockStartableSingleton
                .Setup(m => m.Start())
                .Throws<ApplicationException>();

            Assert.DoesNotThrow(() => 
                testObject.Start()
            );

            mockLogger.Verify(m => m.LogError(It.IsAny<string>(), It.IsAny<ApplicationException>()), Times.Once());            
        }

        [Fact]
        public void shutdown_exceptions_are_swallowed_and_logged()
        {
            Init();

            var mockType = typeof(float);
            var mockTypes = new Type[] { mockType };
            var mockStartableSingleton = new Mock<IStartable>();

            mockReflection
                .Setup(m => m.FindTypesMarkedByAttributes(new Type[] { typeof(SingletonAttribute) }))
                .Returns(mockTypes);

            mockReflection
                .Setup(m => m.GetAttributes<SingletonAttribute>(mockType))
                .Returns(new SingletonAttribute[] { new SingletonAttribute(mockType) });

            mockFactory
                .Setup(m => m.Create(mockType))
                .Returns(mockStartableSingleton.Object);
            mockFactory
                .Setup(m => m.OrderByDeps<SingletonAttribute>(mockTypes, It.IsAny<Func<SingletonAttribute, string>>()))
                .Returns(mockTypes);

            testObject.InstantiateSingletons();

            mockStartableSingleton
                .Setup(m => m.Shutdown())
                .Throws<ApplicationException>();

            Assert.DoesNotThrow(() =>
                testObject.Shutdown()
            );

            mockLogger.Verify(m => m.LogError(It.IsAny<string>(), It.IsAny<ApplicationException>()), Times.Once());
        }
    }
}