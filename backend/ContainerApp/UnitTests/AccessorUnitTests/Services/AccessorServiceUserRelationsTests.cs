using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Accessor.DB;
using Accessor.Models.Users;
using Accessor.Services;
using Dapr.Client;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AccessorUnitTests.Users
{
    public class AccessorServiceUserRelationsTests
    {
        // ---------- EF InMemory ----------
        private static AccessorDbContext NewDb(string name)
        {
            var options = new DbContextOptionsBuilder<AccessorDbContext>()
                .UseInMemoryDatabase(name)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .EnableSensitiveDataLogging()
                .Options;

            return new AccessorDbContext(options);
        }

        // ---------- Config / Service ----------
        private static IConfiguration NewConfig(int ttl = 123) =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
                {
                    ["TaskCache:TTLInSeconds"] = ttl.ToString()
                })
                .Build();

        private static AccessorService NewService(AccessorDbContext db, int ttl = 123)
        {
            var log = Mock.Of<ILogger<AccessorService>>();
            var dapr = new Mock<DaprClient>(MockBehavior.Loose);
            var cfg = NewConfig(ttl);
            return new AccessorService(db, log, dapr.Object, cfg);
        }

        private static UserModel MakeUser(Role role) => new()
        {
            UserId = Guid.NewGuid(),
            Email = $"{Guid.NewGuid():N}@ex.com",
            FirstName = "F",
            LastName = "L",
            Password = "hashed",
            Role = role
        };

        [Fact]
        public async Task GetAllUsersAsync_NoFilter_ReturnsAll()
        {
            var db = NewDb(Guid.NewGuid().ToString());
            var t1 = MakeUser(Role.Teacher);
            var s1 = MakeUser(Role.Student);
            var a1 = MakeUser(Role.Admin);
            db.Users.AddRange(t1, s1, a1);
            await db.SaveChangesAsync();

            var svc = NewService(db);

            var result = await svc.GetAllUsersAsync(roleFilter: null, teacherId: null, ct: CancellationToken.None);
            result.Should().HaveCount(3);
            result.Select(u => u.Role).Should().BeEquivalentTo(new[] { t1.Role, s1.Role, a1.Role });
        }

        [Fact]
        public async Task GetAllUsersAsync_FilterByStudent_ReturnsOnlyStudents()
        {
            var db = NewDb(Guid.NewGuid().ToString());
            db.Users.AddRange(MakeUser(Role.Teacher), MakeUser(Role.Student), MakeUser(Role.Admin), MakeUser(Role.Student));
            await db.SaveChangesAsync();

            var svc = NewService(db);

            var result = await svc.GetAllUsersAsync(Role.Student, teacherId: null, ct: CancellationToken.None);
            result.Should().OnlyContain(u => u.Role == Role.Student);
        }

        [Fact]
        public async Task GetStudentsForTeacherAsync_Empty_ReturnsEmptyEnumeration()
        {
            var db = NewDb(Guid.NewGuid().ToString());
            var teacher = MakeUser(Role.Teacher);
            var otherTeacher = MakeUser(Role.Teacher);
            var student = MakeUser(Role.Student);
            db.Users.AddRange(teacher, otherTeacher, student);
            // No relations added
            await db.SaveChangesAsync();

            var svc = NewService(db);

            var result = await svc.GetStudentsForTeacherAsync(teacher.UserId, CancellationToken.None);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetStudentsForTeacherAsync_ReturnsOnlyStudentsAssignedToTeacher()
        {
            var db = NewDb(Guid.NewGuid().ToString());
            var teacher = MakeUser(Role.Teacher);
            var t2 = MakeUser(Role.Teacher);
            var s1 = MakeUser(Role.Student);
            var s2 = MakeUser(Role.Student);
            var s3 = MakeUser(Role.Student);

            db.Users.AddRange(teacher, t2, s1, s2, s3);

            db.TeacherStudents.AddRange(
                new TeacherStudent { TeacherId = teacher.UserId, StudentId = s1.UserId },
                new TeacherStudent { TeacherId = teacher.UserId, StudentId = s2.UserId },
                new TeacherStudent { TeacherId = t2.UserId, StudentId = s3.UserId } // belongs to other teacher
            );

            await db.SaveChangesAsync();

            var svc = NewService(db);

            var result = (await svc.GetStudentsForTeacherAsync(teacher.UserId, CancellationToken.None)).ToList();
            result.Should().HaveCount(2);
            result.Select(x => x.UserId).Should().BeEquivalentTo(new[] { s1.UserId, s2.UserId });
            result.Should().OnlyContain(u => u.Role == Role.Student);
        }

        [Fact]
        public async Task AssignStudentToTeacherAsync_InvalidRoles_ReturnsFalse()
        {
            var db = NewDb(Guid.NewGuid().ToString());
            var notTeacher = MakeUser(Role.Student);
            var notStudent = MakeUser(Role.Teacher);
            db.Users.AddRange(notTeacher, notStudent);
            await db.SaveChangesAsync();

            var svc = NewService(db);

            var ok = await svc.AssignStudentToTeacherAsync(notTeacher.UserId, notStudent.UserId, CancellationToken.None);
            ok.Should().BeFalse();
        }

        [Fact]
        public async Task AssignStudentToTeacherAsync_NewRelation_PersistsAndReturnsTrue()
        {
            var db = NewDb(Guid.NewGuid().ToString());
            var teacher = MakeUser(Role.Teacher);
            var student = MakeUser(Role.Student);
            db.Users.AddRange(teacher, student);
            await db.SaveChangesAsync();

            var svc = NewService(db);

            var ok = await svc.AssignStudentToTeacherAsync(teacher.UserId, student.UserId, CancellationToken.None);
            ok.Should().BeTrue();

            var rel = await db.TeacherStudents
                .AsNoTracking()
                .FirstOrDefaultAsync(ts => ts.TeacherId == teacher.UserId && ts.StudentId == student.UserId);
            rel.Should().NotBeNull();
        }

        [Fact]
        public async Task AssignStudentToTeacherAsync_ExistingRelation_IsIdempotentAndReturnsTrue()
        {
            var db = NewDb(Guid.NewGuid().ToString());
            var teacher = MakeUser(Role.Teacher);
            var student = MakeUser(Role.Student);
            db.Users.AddRange(teacher, student);
            db.TeacherStudents.Add(new TeacherStudent { TeacherId = teacher.UserId, StudentId = student.UserId });
            await db.SaveChangesAsync();

            var svc = NewService(db);

            var ok = await svc.AssignStudentToTeacherAsync(teacher.UserId, student.UserId, CancellationToken.None);
            ok.Should().BeTrue();

            // Still exactly one relation
            (await db.TeacherStudents.CountAsync(ts => ts.TeacherId == teacher.UserId && ts.StudentId == student.UserId))
                .Should().Be(1);
        }

        [Fact]
        public async Task UnassignStudentFromTeacherAsync_NoRelation_IsIdempotentTrue()
        {
            var db = NewDb(Guid.NewGuid().ToString());
            var teacher = MakeUser(Role.Teacher);
            var student = MakeUser(Role.Student);
            db.Users.AddRange(teacher, student);
            await db.SaveChangesAsync();

            var svc = NewService(db);

            var ok = await svc.UnassignStudentFromTeacherAsync(teacher.UserId, student.UserId, CancellationToken.None);
            ok.Should().BeTrue();
        }

        [Fact]
        public async Task UnassignStudentFromTeacherAsync_RemovesExistingRelation_ReturnsTrue()
        {
            var db = NewDb(Guid.NewGuid().ToString());
            var teacher = MakeUser(Role.Teacher);
            var student = MakeUser(Role.Student);
            db.Users.AddRange(teacher, student);
            db.TeacherStudents.Add(new TeacherStudent { TeacherId = teacher.UserId, StudentId = student.UserId });
            await db.SaveChangesAsync();

            var svc = NewService(db);

            var ok = await svc.UnassignStudentFromTeacherAsync(teacher.UserId, student.UserId, CancellationToken.None);
            ok.Should().BeTrue();

            (await db.TeacherStudents.AnyAsync(ts => ts.TeacherId == teacher.UserId && ts.StudentId == student.UserId))
                .Should().BeFalse();
        }

        [Fact]
        public async Task GetTeachersForStudentAsync_Empty_ReturnsEmptyEnumeration()
        {
            var db = NewDb(Guid.NewGuid().ToString());
            var student = MakeUser(Role.Student);
            var teacher = MakeUser(Role.Teacher);
            db.Users.AddRange(student, teacher);
            // no relation
            await db.SaveChangesAsync();

            var svc = NewService(db);

            var result = await svc.GetTeachersForStudentAsync(student.UserId, CancellationToken.None);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTeachersForStudentAsync_ReturnsOnlyTeachersAssignedToStudent()
        {
            var db = NewDb(Guid.NewGuid().ToString());
            var s1 = MakeUser(Role.Student);
            var t1 = MakeUser(Role.Teacher);
            var t2 = MakeUser(Role.Teacher);
            var t3 = MakeUser(Role.Teacher);
            db.Users.AddRange(s1, t1, t2, t3);

            db.TeacherStudents.AddRange(
                new TeacherStudent { TeacherId = t1.UserId, StudentId = s1.UserId },
                new TeacherStudent { TeacherId = t2.UserId, StudentId = s1.UserId }
                // t3 not assigned
            );

            await db.SaveChangesAsync();

            var svc = NewService(db);

            var result = (await svc.GetTeachersForStudentAsync(s1.UserId, CancellationToken.None)).ToList();
            result.Should().HaveCount(2);
            result.Select(r => r.UserId).Should().BeEquivalentTo(new[] { t1.UserId, t2.UserId });
            result.Should().OnlyContain(u => u.Role == Role.Teacher);
        }
    }
}
