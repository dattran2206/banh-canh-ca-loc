-- Tạo Database staging nếu chưa có
CREATE DATABASE IF NOT EXISTS `staging` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Sử dụng database stub và tạo bảng
USE `stub`;
CREATE TABLE IF NOT EXISTS `stub_data` (
  `id` int NOT NULL AUTO_INCREMENT,
  `path` text,
  `input_json` longtext,
  `output_json` longtext,
  `header_json` longtext,
  `http_status_cd` int DEFAULT '200',
  `response_add_time` int DEFAULT '0',
  `memo` text,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Sử dụng database staging và tạo bảng
USE `staging`;
CREATE TABLE IF NOT EXISTS `stub_data` (
  `id` int NOT NULL AUTO_INCREMENT,
  `path` text,
  `input_json` longtext,
  `output_json` longtext,
  `header_json` longtext,
  `http_status_cd` int DEFAULT '200',
  `response_add_time` int DEFAULT '0',
  `memo` text,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
