-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';


CREATE TABLE IF NOT EXISTS `L5192_2`.`User` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Username` VARCHAR(20) NOT NULL,
  `Password` VARCHAR(255) NOT NULL,
  `Firstname` VARCHAR(25) NOT NULL,
  `Lastname` VARCHAR(50) NOT NULL,
  `Role` VARCHAR(50) NOT NULL,
  PRIMARY KEY (`Id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`Project`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `L5192_2`.`Project` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(50) NOT NULL,
  `Manager` INT NOT NULL,
  `Expires` DATE NULL,
  PRIMARY KEY (`Id`),
  INDEX `fk_Project_User1_idx` (`Manager` ASC),
  CONSTRAINT `fk_Project_User1`
    FOREIGN KEY (`Manager`)
    REFERENCES `L5192_2`.`User` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`Tasks`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `L5192_2`.`Tasks` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(25) NOT NULL,
  `UserId` INT NOT NULL,
  `ProjectId` INT NOT NULL,
  `Descr` VARCHAR(255) NOT NULL,
  `Status` VARCHAR(20) NOT NULL,
  `Expires` DATE NULL,
  PRIMARY KEY (`Id`),
  INDEX `fk_Tasks_User_idx` (`UserId` ASC),
  INDEX `fk_Tasks_Project1_idx` (`ProjectId` ASC),
  CONSTRAINT `fk_Tasks_User`
    FOREIGN KEY (`UserId`)
    REFERENCES `L5192_2`.`User` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Tasks_Project1`
    FOREIGN KEY (`ProjectId`)
    REFERENCES `L5192_2`.`Project` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`ProjectTeam`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `L5192_2`.`ProjectTeam` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `UserId` INT NOT NULL,
  `ProjectId` INT NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `fk_ProjectTeam_User1_idx` (`UserId` ASC),
  INDEX `fk_ProjectTeam_Project1_idx` (`ProjectId` ASC),
  CONSTRAINT `fk_ProjectTeam_User1`
    FOREIGN KEY (`UserId`)
    REFERENCES `L5192_2`.`User` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_ProjectTeam_Project1`
    FOREIGN KEY (`ProjectId`)
    REFERENCES `L5192_2`.`Project` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;