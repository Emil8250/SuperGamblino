﻿using System;
using DSharpPlus.Entities;
using Moq;
using SuperGamblino.Infrastructure.Connectors;
using SuperGamblino.Infrastructure.DatabasesObjects;
using Xunit;

namespace SuperGamblino.Commands.Tests
{
    public class HourlyRewardTests
    {
        private HourlyRewardCommandLogic GetHourlyRewardCommandLogic(UsersConnector usersConnector)
        {
            return new HourlyRewardCommandLogic(usersConnector, Helpers.GetMessages());
        }

        [Fact]
        public async void GiveRewardOnFirstUser()
        {
            var amountOfCredits = 0;
            var usersConnector = Helpers.GetDatabaseConnector<UsersConnector>();
            usersConnector.Setup(x => x.GetDateTime(0, It.IsAny<string>()))
                .ReturnsAsync(new DateTimeResult(true, null));
            usersConnector.Setup(x => x.CommandGiveCredits(0, It.IsAny<int>()))
                .Callback((ulong user, int credits) => { amountOfCredits += credits; });
            usersConnector.Setup(x => x.SetDateTime(0, It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(true);
            var logic = GetHourlyRewardCommandLogic(usersConnector.Object);

            var result = await logic.GetHourlyReward(0);

            Assert.Equal(Helpers.SuccessColor, result.Color);
            Assert.Equal("HourlyReward", result.Title);
            Assert.Equal($"You've gained {amountOfCredits} credits!", result.Description);
            Assert.NotEqual(0, amountOfCredits);
        }

        [Fact]
        public async void GiveRewardOnLaterUse()
        {
            var amountOfCredits = 0;
            var usersConnector = Helpers.GetDatabaseConnector<UsersConnector>();
            usersConnector.Setup(x => x.GetDateTime(0, It.IsAny<string>()))
                .ReturnsAsync(new DateTimeResult(true, new DateTime(2020, 1, 1)));
            usersConnector.Setup(x => x.CommandGiveCredits(0, It.IsAny<int>()))
                .Callback((ulong user, int credits) => { amountOfCredits += credits; });
            usersConnector.Setup(x => x.SetDateTime(0, It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(true);
            var logic = GetHourlyRewardCommandLogic(usersConnector.Object);

            var result = await logic.GetHourlyReward(0);

            Assert.Equal(Helpers.SuccessColor, result.Color);
            Assert.Equal("HourlyReward", result.Title);
            Assert.Equal($"You've gained {amountOfCredits} credits!", result.Description);
            Assert.NotEqual(0, amountOfCredits);
        }

        [Fact]
        public async void InformAboutCallingCommandTooEarly()
        {
            var usersConnector = Helpers.GetDatabaseConnector<UsersConnector>();
            usersConnector.Setup(x => x.GetDateTime(0, It.IsAny<string>()))
                .ReturnsAsync(new DateTimeResult(true, DateTime.Now));
            var logic = GetHourlyRewardCommandLogic(usersConnector.Object);

            var result = await logic.GetHourlyReward(0);

            Assert.Equal(Helpers.WarningColor, result.Color);
            Assert.Equal("HourlyReward", result.Title);
            Assert.Matches(
                "You've tried to execute command '!hourly' before it was ready! Command will be ready in ([^\\s]+)",
                result.Description);
        }

        [Fact]
        public async void InformAboutProblemsWithDb()
        {
            var usersConnector = Helpers.GetDatabaseConnector<UsersConnector>();
            usersConnector.Setup(x => x.GetDateTime(0, It.IsAny<string>()))
                .ReturnsAsync(new DateTimeResult(false, null));
            var logic = GetHourlyRewardCommandLogic(usersConnector.Object);

            var result = await logic.GetHourlyReward(0);

            Assert.Equal(Helpers.WarningColor, result.Color);
            Assert.Equal("Some problem with DB occured!", result.Description);
        }
    }
}