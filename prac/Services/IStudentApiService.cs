using StudentClient.Models;
using WebServiceClients;

namespace StudentClient.Services;

public interface IStudentApiService
{
    Task<ApiResult<List<Student>>> GetAllAsync();
    Task<ApiResult<Student>> GetByIdAsync(int id);
    Task<ApiResult<bool>> CreateAsync(Student item);
    Task<ApiResult<bool>> UpdateAsync(Student item);
    Task<ApiResult<bool>> DeleteAsync(int id);
}
