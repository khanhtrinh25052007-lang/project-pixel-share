USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'PhotoSharingDB')
BEGIN
    ALTER DATABASE PhotoSharingDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PhotoSharingDB;
END
GO

CREATE DATABASE PhotoSharingDB;
GO

USE PhotoSharingDB;
GO

-- 1. BẢNG USERS (NGƯỜI DÙNG)
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    AvatarUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- 2. BẢNG PHOTOPOSTS (BÀI ĐĂNG ẢNH & CAPTION)
CREATE TABLE PhotoPosts (
    PostId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    Caption NVARCHAR(1000) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_PhotoPosts_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- 3. BẢNG COMMENTS (BÌNH LUẬN)
CREATE TABLE Comments (
    CommentId INT IDENTITY(1,1) PRIMARY KEY,
    PostId INT NOT NULL,
    UserId INT NOT NULL,
    Content NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Comments_PhotoPosts FOREIGN KEY (PostId) REFERENCES PhotoPosts(PostId) ON DELETE CASCADE,
    CONSTRAINT FK_Comments_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- 4. BẢNG FOLLOWS (THEO DÕI)
CREATE TABLE Follows (
    FollowId INT IDENTITY(1,1) PRIMARY KEY,
    FollowerId INT NOT NULL,
    FollowingId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Follows_Follower FOREIGN KEY (FollowerId) REFERENCES Users(UserId),
    CONSTRAINT FK_Follows_Following FOREIGN KEY (FollowingId) REFERENCES Users(UserId),
    CONSTRAINT UQ_Follow_Pair UNIQUE (FollowerId, FollowingId)
);
GO
CREATE TABLE Likes (
    LikeId INT IDENTITY(1,1) PRIMARY KEY,
    PostId INT NOT NULL,
    UserId INT NOT NULL,
    FOREIGN KEY (PostId) REFERENCES PhotoPosts(PostId) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
 
-- CHÈN DỮ LIỆU MẪU
INSERT INTO Users (Username, Email, PasswordHash, AvatarUrl, CreatedAt) VALUES
(N'ban_cua_toi', N'me@huflit.edu.vn', N'123456', N'', GETDATE()),
(N'linh_nguyen', N'linh@gmail.com', N'123456', N'', GETDATE()),
(N'minh_tuan', N'tuan@gmail.com', N'123456', N'', GETDATE());
GO

INSERT INTO Follows (FollowerId, FollowingId, CreatedAt) VALUES
(1, 2, GETDATE());
GO