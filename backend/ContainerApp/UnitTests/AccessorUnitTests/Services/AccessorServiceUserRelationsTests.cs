using Accessor.Models.Users;
using Accessor.Services;
using Accessor.Services.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using AccessorUnitTests.Shared;

namespace AccessorUnitTests.Users
{
    [Collection("SharedDb")]
    public class AccessorServiceUserRelationsTests
    {
        private readonly SharedDbFixture _fixture;

        public AccessorServiceUserRelationsTests(SharedDbFixture fixture)
        {
            _fixture = fixture;
        }

        private IUserService NewService() =>
            new UserService(_fixture.Db, new Mock<ILogger<UserService>>(MockBehavior.Loose).Object);

        private static UserModel MakeUser(Role role) => new()
        {
            UserId = Guid.NewGuid(),
            Email = $"{Guid.NewGuid():N}@ex.com",
            FirstName = "F",
            LastName = "L",
            Password = "hashed",
            Role = role,
            Interests = []
        };

        [Fact]
        public async Task GetAllUsersAsync_NoFilter_ReturnsAll()
        {
            await _fixture.ResetAsync();
            var db = _fixture.Db;

            var t1 = MakeUser(Role.Teacher);
            var s1 = MakeUser(Role.Student);
            var a1 = MakeUser(Role.Admin);
            db.Users.AddRange(t1, s1, a1);
            await db.SaveChangesAsync();

            var svc = NewService();

            var result = await svc.GetAllUsersAsync(roleFilter: null, teacherId: null, ct: CancellationToken.None);
            result.Should().HaveCount(3);
            result.Select(u => u.Role).Should().BeEquivalentTo(new[] { t1.Role, s1.Role, a1.Role });
        }

        [Fact]
        public async Task GetAllUsersAsync_FilterByStudent_ReturnsOnlyStudents()
        {
            await _fixture.ResetAsync();
            var db = _fixture.Db;

            db.Users.AddRange(
                MakeUser(Role.Teacher),
                MakeUser(Role.Student),
                MakeUser(Role.Admin),
                MakeUser(Role.Student)
            );
            await db.SaveChangesAsync();

            var svc = NewService();

            var result = await svc.GetAllUsersAsync(Role.Student, teacherId: null, ct: CancellationToken.None);
            result.Should().OnlyContain(u => u.Role == Role.Student);
        }

        [Fact]
        public async Task GetStudentsForTeacherAsync_Empty_ReturnsEmptyEnumeration()
        {
            await _fixture.ResetAsync();
            var db = _fixture.Db;

            var teacher = MakeUser(Role.Teacher);
            var otherTeacher = MakeUser(Role.Teacher);
            var student = MakeUser(Role.Student);
            db.Users.AddRange(teacher, otherTeacher, student);
            // no TeacherStudents relations
            await db.SaveChangesAsync();

            var svc = NewService();

            var result = await svc.GetStudentsForTeacherAsync(teacher.UserId, CancellationToken.None);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetStudentsForTeacherAsync_ReturnsOnlyStudentsAssignedToTeacher()
        {
            await _fixture.ResetAsync();
            var db = _fixture.Db;

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

            var svc = NewService();

            var result = (await svc.GetStudentsForTeacherAsync(teacher.UserId, CancellationToken.None)).ToList();
            result.Should().HaveCount(2);
            result.Select(x => x.UserId).Should().BeEquivalentTo(new[] { s1.UserId, s2.UserId });
            result.Should().OnlyContain(u => u.Role == Role.Student);
        }

        [Fact]
        public async Task AssignStudentToTeacherAsync_InvalidRoles_ReturnsFalse()
        {
            await _fixture.ResetAsync();
            var db = _fixture.Db;

            var notTeacher = MakeUser(Role.Student);
            var notStudent = MakeUser(Role.Teacher);
            db.Users.AddRange(notTeacher, notStudent);
            await db.SaveChangesAsync();

            var svc = NewService();

            var ok = await svc.AssignStudentToTeacherAsync(notTeacher.UserId, notStudent.UserId, CancellationToken.None);
            ok.Should().BeFalse();
        }

        [Fact]
        public async Task AssignStudentToTeacherAsync_NewRelation_PersistsAndReturnsTrue()
        {
            await _fixture.ResetAsync();
            var db = _fixture.Db;

            var teacher = MakeUser(Role.Teacher);
            var student = MakeUser(Role.Student);
            db.Users.AddRange(teacher, student);
            await db.SaveChangesAsync();

            var svc = NewService();

            var ok = await svc.AssignStudentToTeacherAsync(teacher.UserId, student.UserId, CancellationToken.None);
            ok.Should().BeTrue();

            var rel = await db.TeacherStudents
                .AsNoTracking()
                .FirstOrDefaultAsync(ts => ts.TeacherId == teacher.UserId && ts.StudentId == student.UserId);
            rel.Should().NotBeNull();
        }

        [Fact]
        public async Task UnassignStudentFromTeacherAsync_NoRelation_IsIdempotentTrue()
        {
            await _fixture.ResetAsync();
            var db = _fixture.Db;

            var teacher = MakeUser(Role.Teacher);
            var student = MakeUser(Role.Student);
            db.Users.AddRange(teacher, student);
            await db.SaveChangesAsync();

            var svc = NewService();

            var ok = await svc.UnassignStudentFromTeacherAsync(teacher.UserId, student.UserId, CancellationToken.None);
            ok.Should().BeTrue();
        }

        [Fact]
        public async Task UnassignStudentFromTeacherAsync_RemovesExistingRelation_ReturnsTrue()
        {
            await _fixture.ResetAsync();
            var db = _fixture.Db;

            var teacher = MakeUser(Role.Teacher);
            var student = MakeUser(Role.Student);
            db.Users.AddRange(teacher, student);
            db.TeacherStudents.Add(new TeacherStudent { TeacherId = teacher.UserId, StudentId = student.UserId });
            await db.SaveChangesAsync();

            var svc = NewService();

            var ok = await svc.UnassignStudentFromTeacherAsync(teacher.UserId, student.UserId, CancellationToken.None);
            ok.Should().BeTrue();

            (await db.TeacherStudents.AnyAsync(ts => ts.TeacherId == teacher.UserId && ts.StudentId == student.UserId))
                .Should().BeFalse();
        }

        [Fact]
        public async Task GetTeachersForStudentAsync_Empty_ReturnsEmptyEnumeration()
        {
            await _fixture.ResetAsync();
            var db = _fixture.Db;

            var student = MakeUser(Role.Student);
            var teacher = MakeUser(Role.Teacher);
            db.Users.AddRange(student, teacher);
            // no relation
            await db.SaveChangesAsync();

            var svc = NewService();

            var result = await svc.GetTeachersForStudentAsync(student.UserId, CancellationToken.None);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTeachersForStudentAsync_ReturnsOnlyTeachersAssignedToStudent()
        {
            await _fixture.ResetAsync();
            var db = _fixture.Db;

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

            var svc = NewService();

            var result = (await svc.GetTeachersForStudentAsync(s1.UserId, CancellationToken.None)).ToList();
            result.Should().HaveCount(2);
            result.Select(r => r.UserId).Should().BeEquivalentTo(new[] { t1.UserId, t2.UserId });
            result.Should().OnlyContain(u => u.Role == Role.Teacher);
        }
    }
}
