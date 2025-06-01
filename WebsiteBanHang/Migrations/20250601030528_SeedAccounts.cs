using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBanHang.Migrations
{
    /// <summary>
    /// Migration để tự động tạo tài khoản Admin và User
    /// </summary>
    public partial class SeedAccounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Tạo Roles
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Admin')
                INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) 
                VALUES ('1', 'Admin', 'ADMIN', NEWID());
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Customer')
                INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) 
                VALUES ('2', 'Customer', 'CUSTOMER', NEWID());
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Company')
                INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) 
                VALUES ('3', 'Company', 'COMPANY', NEWID());
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Employee')
                INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) 
                VALUES ('4', 'Employee', 'EMPLOYEE', NEWID());
            ");

            // 2. Tạo Admin User
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM AspNetUsers WHERE Email = 'admin@example.com')
                INSERT INTO AspNetUsers (
                    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
                    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed,
                    TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount,
                    FullName, Address, Age
                ) VALUES (
                    'admin-user-id',
                    'admin@example.com',
                    'ADMIN@EXAMPLE.COM',
                    'admin@example.com', 
                    'ADMIN@EXAMPLE.COM',
                    1,
                    'AQAAAAEAACcQAAAAEJ5Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q==',
                    NEWID(),
                    NEWID(),
                    NULL,
                    0,
                    0,
                    NULL,
                    1,
                    0,
                    N'Quản trị viên',
                    N'123 Admin Street, TP.HCM',
                    '30'
                );
            ");

            // 3. Tạo Customer User  
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM AspNetUsers WHERE Email = 'user@example.com')
                INSERT INTO AspNetUsers (
                    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
                    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed,
                    TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount,
                    FullName, Address, Age
                ) VALUES (
                    'customer-user-id',
                    'user@example.com',
                    'USER@EXAMPLE.COM',
                    'user@example.com',
                    'USER@EXAMPLE.COM',
                    1,
                    'AQAAAAEAACcQAAAAEJ5Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q1Q==',
                    NEWID(),
                    NEWID(),
                    NULL,
                    0,
                    0,
                    NULL,
                    1,
                    0,
                    N'Nguyễn Văn A',
                    N'456 User Street, TP.HCM', 
                    '25'
                );
            ");

            // 4. Gán Role Admin cho Admin User
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM AspNetUserRoles WHERE UserId = 'admin-user-id' AND RoleId = '1')
                INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('admin-user-id', '1');
            ");

            // 5. Gán Role Customer cho Customer User
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM AspNetUserRoles WHERE UserId = 'customer-user-id' AND RoleId = '2')
                INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('customer-user-id', '2');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa UserRoles
            migrationBuilder.Sql("DELETE FROM AspNetUserRoles WHERE UserId IN ('admin-user-id', 'customer-user-id')");

            // Xóa Users
            migrationBuilder.Sql("DELETE FROM AspNetUsers WHERE Id IN ('admin-user-id', 'customer-user-id')");

            // Xóa Roles  
            migrationBuilder.Sql("DELETE FROM AspNetRoles WHERE Id IN ('1', '2', '3', '4')");
        }
    }
}