START TRANSACTION;

CREATE TABLE `shoppinglists` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `FamilyId` int NOT NULL,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_shoppinglists` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_shoppinglists_families_FamilyId` FOREIGN KEY (`FamilyId`) REFERENCES `families` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `shoppingitems` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ListId` int NOT NULL,
    `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Quantity` double NOT NULL,
    `Unit` varchar(50) CHARACTER SET utf8mb4 NULL,
    `IsBought` tinyint(1) NOT NULL,
    `BuyerId` int NULL,
    `CreatedByUserId` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_shoppingitems` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_shoppingitems_shoppinglists_ListId` FOREIGN KEY (`ListId`) REFERENCES `shoppinglists` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_shoppingitems_users_BuyerId` FOREIGN KEY (`BuyerId`) REFERENCES `users` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_shoppingitems_users_CreatedByUserId` FOREIGN KEY (`CreatedByUserId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

UPDATE `rewards` SET `Title` = 'Mua đồ ăn vặt'
WHERE `Id` = -3;
SELECT ROW_COUNT();


UPDATE `rewards` SET `Title` = 'Chơi game 1 giờ'
WHERE `Id` = -2;
SELECT ROW_COUNT();


CREATE INDEX `IX_shoppingitems_BuyerId` ON `shoppingitems` (`BuyerId`);

CREATE INDEX `IX_shoppingitems_CreatedByUserId` ON `shoppingitems` (`CreatedByUserId`);

CREATE INDEX `IX_shoppingitems_ListId` ON `shoppingitems` (`ListId`);

CREATE INDEX `IX_shoppinglists_FamilyId` ON `shoppinglists` (`FamilyId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260626090218_UpdateSchema_20260626', '8.0.0');

COMMIT;

