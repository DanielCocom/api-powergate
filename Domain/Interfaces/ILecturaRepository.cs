using api_powergate.Domain.Models;

namespace api_powergate.Domain.Interfaces
{
    public interface ILecturaRepository
    {

        /* no devuelve valor */
         Task AddLectura(Lectura lectura);
    }
}
