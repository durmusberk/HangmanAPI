using System;
using Hangman.Models;
using Hangman.Data;

namespace Hangman.Data
{
    public class UnitOfWork : IDisposable
    {
        private readonly ApplicationDbContext _db = new();
        private GenericRepository<Session>? sessionRepository;
        private GenericRepository<User>? userRepository;

        public GenericRepository<User> UserRepository
        {
            get
            {

                userRepository ??= new GenericRepository<User>(_db);
                return userRepository;
            }
        }

        public GenericRepository<Session> SessionRepository
        {
            get
            {

                sessionRepository ??= new GenericRepository<Session>(_db);
                return sessionRepository;
            }
        }

        public void SaveAsync()
        {
            _db.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                _db.Dispose();
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}