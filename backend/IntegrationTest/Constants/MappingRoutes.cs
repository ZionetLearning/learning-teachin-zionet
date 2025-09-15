namespace IntegrationTests.Constants;

public static class MappingRoutes
{
    private const string Prefix = "users-manager";

    // POST /users-manager/teacher/{teacherId}/students/{studentId}
    public static string Assign(Guid teacherId, Guid studentId)
        => $"{Prefix}/teacher/{teacherId:D}/students/{studentId:D}";

    // DELETE /users-manager/teacher/{teacherId}/students/{studentId}
    public static string Unassign(Guid teacherId, Guid studentId)
        => $"{Prefix}/teacher/{teacherId:D}/students/{studentId:D}";

    // GET /users-manager/teacher/{teacherId}/students
    public static string ListStudents(Guid teacherId)
        => $"{Prefix}/teacher/{teacherId:D}/students";

    // GET /users-manager/student/{studentId}/teachers
    public static string ListTeachers(Guid studentId)
        => $"{Prefix}/student/{studentId:D}/teachers";
}
