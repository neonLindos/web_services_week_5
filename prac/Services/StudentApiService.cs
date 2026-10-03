using StudentClient.Models;
using WebServiceClients;

namespace StudentClient.Services;

public class StudentApiService : ApiService<Student>, IStudentApiService
{
    public StudentApiService(IHttpClientFactory factory)
        : base(factory.CreateClient("StudentApi"), "api/students", "Студент с указанным ID не найден.") { }

    public Task<ApiResult<bool>> UpdateAsync(Student item) => UpdateAsync(item.Id, item);
}
