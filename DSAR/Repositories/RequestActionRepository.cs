﻿using Azure.Core;
using DSAR.Data;
using DSAR.Interfaces;
using DSAR.Models;
using DSAR.Repository;
using DSAR.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DSAR.Repositories
{
    public class RequestActionRepository : IRequestActionRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly iFormRepository _formRepo;
        private readonly IAppHistoryRepository _historyRepository;
        public RequestActionRepository(AppDbContext context, UserManager<User> userManager, iFormRepository formRepo, IAppHistoryRepository historyRepository)
        {
            _context = context;
            _userManager = userManager;
            _formRepo = formRepo;
            _historyRepository = historyRepository;
        }

        public RequestActions Create(RequestViewModel viewModel, User currentUser, FormData request)
        {


            var requestActions = new RequestActions
            {
                RequestId = request.RequestId,
                UserId = currentUser.Id,
                DepartmentId = currentUser.DepartmentId,
                SectionId = currentUser.SectionId,
                StatusId = 1,
                LevelId = 1
            };

            _context.RequestActions.Add(requestActions);
            _context.SaveChanges();
            return requestActions;
        }
        public RequestActions CreateSectionManager(RequestViewModel viewModel, User currentUser, FormData request)
        {


            var requestActions = new RequestActions
            {
                RequestId = request.RequestId,
                UserId = currentUser.Id,
                DepartmentId = currentUser.DepartmentId,
                SectionId = currentUser.SectionId,
                StatusId = 1,
                LevelId = 2
            };

            _context.RequestActions.Add(requestActions);
            _context.SaveChanges();
            return requestActions;
        }

        public RequestActions CreateDepartmentManager(RequestViewModel viewModel, User currentUser, FormData request)
        {


            var requestActions = new RequestActions
            {
                RequestId = request.RequestId,
                UserId = currentUser.Id,
                DepartmentId = currentUser.DepartmentId,
                SectionId = currentUser.SectionId,
                StatusId = 1,
                LevelId = 3
            };

            _context.RequestActions.Add(requestActions);
            _context.SaveChanges();
            return requestActions;
        }

        public RequestActions CreateITManager(RequestViewModel viewModel, User currentUser, FormData request)
        {


            var requestActions = new RequestActions
            {
                RequestId = request.RequestId,
                UserId = currentUser.Id,
                DepartmentId = currentUser.DepartmentId,
                SectionId = currentUser.SectionId,
                StatusId = 1,
                LevelId = 4
            };

            _context.RequestActions.Add(requestActions);
            _context.SaveChanges();
            return requestActions;
        }


        public void Create(RequestActions request)
        {
            throw new NotImplementedException();
        }

        public void Delete(string Id)
        {
            var req = _context.RequestActions.Find(Id);
            if (req == null)
                throw new InvalidOperationException("User not found.");

            _context.RequestActions.Remove(req);
            _context.SaveChanges();
        }

        public IEnumerable<RequestActions> GetAll()
        {
            return _context.RequestActions
         .Include(r => r.Status)
         .Include(r => r.Department)
         .Include(r => r.User) // assuming RequestActions.User navigation exists
         .ToList();
        }
        public IEnumerable<RequestActions> GetAllByUserId(string userId)
        {
            return _context.RequestActions
                .Include(r => r.User)
                .Include(r => r.Status)
                .Include(r => r.Department)
                .Include(r => r.FormData)
                .Where(r => r.UserId == userId)
                .ToList();
        }
        public RequestActions GetById(string Id)
        {
            var req = _context.RequestActions.Find(Id);
            return req;
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void UpdateLevel(RequestActions request)
        {
            // Fix for CS0136 and CS0841: Rename the variable to avoid conflict with the parameter name.
            var existingRequest = _context.RequestActions.Find(request.ActionId);
            if (existingRequest == null)
            {
                throw new InvalidOperationException("Request not found.");
            }

            // Update properties of the existing request.
            existingRequest.StatusId = request.StatusId;
            existingRequest.LevelId = request.LevelId;
            existingRequest.DepartmentId = request.DepartmentId;
            existingRequest.UserId = request.UserId;

            // Save changes.
            _context.RequestActions.Update(existingRequest);
            _context.SaveChanges();
        }
        public async Task<RequestActions> GetRequestActionByRequestIdAsync(int requestId)
        {
            return await _context.RequestActions
                       .FirstOrDefaultAsync(ra => ra.RequestId == requestId);

        }
        public async Task<RequestActions?> GetByIdAsync(int id)
        {
            return await _context.RequestActions.FindAsync(id);
        }

        public async Task<List<RequestActions>> GetCompeleteRequestsByUserId(string UserId)
        {
            return await _context.RequestActions
                .Include(r => r.User)
                .Include(r => r.Status)
                .Include(r => r.Department)
                .Include(r => r.FormData)
                .Where(r => r.UserId == UserId && (r.LevelId == 8 || r.LevelId == 9)) // Assuming 2 is the ID for completed status
                .ToListAsync();

        }

        public async Task<List<RequestActions>> GetRequestsStillInProcessByUserId(string UserId)
        {
            return await _context.RequestActions
                .Include(r => r.User)
                .Include(r => r.Status)
                .Include(r => r.Department)
                .Include(r => r.FormData)
                .Where(r => r.UserId == UserId && r.LevelId != 8 && r.LevelId != 9)
                .ToListAsync();

        }

        public async Task<bool> ProtectViewPages(int id, User currentUser, FormData request, RequestActions requestActions)
        {
            int currentDepartmentId = currentUser.DepartmentId;
            int requestDepartmentId = requestActions.DepartmentId;

            // Get roles
            bool isSectionManager = await _userManager.IsInRoleAsync(currentUser, "SectionManager");
            bool isDepartmentManager = await _userManager.IsInRoleAsync(currentUser, "DepartmentManager");
            bool isITManager = await _userManager.IsInRoleAsync(currentUser, "ITManager");
            bool isApplicationManager = await _userManager.IsInRoleAsync(currentUser, "ApplicationManager");
            bool isAnalyzer = await _userManager.IsInRoleAsync(currentUser, "Analyzer");
            bool isUser = await _userManager.IsInRoleAsync(currentUser, "User");
            var histories = await _historyRepository.GetHistoriesByRequestIdAsync(request.RequestId);
            bool isAnyActor = histories.Any(h => h.UserId == currentUser.Id);

            if (isUser && request.UserId != currentUser.Id)
            {
                return false;
            }

            // ✅ Check department match for all managers/analyzers
            if ((isSectionManager || isDepartmentManager || isITManager || isAnalyzer) &&
                requestDepartmentId != currentDepartmentId)
            {
                return false;
            }

            // Get all history entries for this request


            // ... your role checks ...

            if (isSectionManager && requestActions.LevelId != 1 && !isAnyActor)
                return false;

            if (isDepartmentManager && requestActions.LevelId != 2 && !isAnyActor)
                return false;

            if (isITManager && !(requestActions.LevelId == 3 || requestActions.LevelId == 7) && !isAnyActor)
                return false;

            if (isApplicationManager && !(requestActions.LevelId == 4 || requestActions.LevelId == 6) && !isAnyActor)
                return false;

            if (isAnalyzer && requestActions.LevelId != 5 && !isAnyActor)
                return false;

            // Final fallback
            if (!isUser && !isSectionManager && !isDepartmentManager && !isITManager && !isApplicationManager && !isAnalyzer && !isAnyActor)
            {
                return false;
            }

            // Get the last history entry for this request

            // ✅ Final fallback check: allow if user is in any allowed role OR is the last actor
            if (!isUser && !isSectionManager && !isDepartmentManager && !isITManager && !isApplicationManager && !isAnalyzer && !isAnyActor)
            {
                return false;
            }

            return true;
        }

     
    }
}

