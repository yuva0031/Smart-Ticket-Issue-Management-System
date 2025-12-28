using System.Security.Cryptography;
using System.Text;

using AutoMapper;

using SmartTicketSystem.Application.DTOs.Auth;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Infrastructure.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly IMapper _mapper;
    private readonly JwtService _jwt;

    public AuthService(IUserRepository repo, IMapper mapper, JwtService jwt)
    {
        _repo = repo;
        _mapper = mapper;
        _jwt = jwt;
    }

    public async Task<string> Register(RegisterUserDto registerUserDto)
    {
        var exists = await _repo.GetByEmail(registerUserDto.Email);
        if (exists != null)
            return "User already exists";

        CreatePassword(registerUserDto.Password, out byte[] hash, out byte[] salt);

        var user = _mapper.Map<User>(registerUserDto);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.Profile = _mapper.Map<UserProfile>(registerUserDto);

        await _repo.AddUser(user);
        await _repo.Save();

        user.UserRoles = registerUserDto.RoleIds.Select(id => new UserRole
        {
            UserId = user.Id,
            RoleId = id
        }).ToList();

        await _repo.Save();
        return "Registered Successfully";
    }

    public async Task<AuthResponseDto?> Login(LoginDto dto)
    {
        var user = await _repo.GetByEmailWithRoles(dto.Email);
        if (user == null || !VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt))
            return null;

        return new AuthResponseDto
        {
            Name = user.Name,
            Roles = user.UserRoles.Select(x => x.Role.RoleName).ToList(),
            Token = _jwt.GenerateToken(user)
        };
    }

    private void CreatePassword(string password, out byte[] hash, out byte[] salt)
    {
        using var hmac = new HMACSHA512();
        salt = hmac.Key;
        hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private bool VerifyPassword(string password, byte[] hash, byte[] salt)
    {
        using var hmac = new HMACSHA512(salt);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(password)).SequenceEqual(hash);
    }
}

