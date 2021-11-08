using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EPiServer.Marketing.KPI.Dal;
using EPiServer.Marketing.KPI.Dal.Model;
using EPiServer.Marketing.KPI.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Data.SqlClient;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.KPI.DataAccess
{
    internal class KpiDataAccess : IKpiDataAccess
    {
        public readonly Injected<IRepository> _repository;
        internal bool _UseEntityFramework;

        [ExcludeFromCodeCoverage]
        public KpiDataAccess()
        {
            if (!HasTableNamed((BaseRepository)_repository.Service, "tblKeyPerformaceIndicator"))
            {
                // the sql scripts need to be run!
                throw new DatabaseDoesNotExistException();
            }
        }

        /// <summary>
        /// Deletes KPI object from the DB.
        /// </summary>
        /// <param name="kpiId">ID of the KPI to delete.</param>
        public void Delete(Guid kpiId)
        {
            DeleteHelper(_repository.Service, kpiId);
        }

        private void DeleteHelper(IRepository repo, Guid kpiId)
        {
            repo.DeleteKpi(kpiId);
            repo.SaveChanges();
        }

        /// <summary>
        /// Returns a KPI object based on its ID.
        /// </summary>
        /// <param name="kpiId">ID of the KPI to retrieve.</param>
        /// <returns>KPI object.</returns>
        public IDalKpi Get(Guid kpiId)
        {
            return GetHelper(_repository.Service, kpiId);
        }

        private IDalKpi GetHelper(IRepository repo, Guid kpiId)
        {
            return repo.GetById(kpiId);
        }

        /// <summary>
        /// Gets the whole list of KPI objects.
        /// </summary>
        /// <returns>List of KPI objects.</returns>
        public List<IDalKpi> GetKpiList()
        {
            return GetKpiListHelper(_repository.Service);
        }

        private List<IDalKpi> GetKpiListHelper(IRepository repo)
        {
            return repo.GetAll().ToList();
        }

        /// <summary>
        /// Adds or updates a KPI object.
        /// </summary>
        /// <param name="kpiObject">ID of the KPI to add/update.</param>
        /// <returns>The ID of the KPI object that was added/updated.</returns>
        public Guid Save(IDalKpi kpiObject)
        {
            return Save(new List<IDalKpi>() { kpiObject }).First();
        }

        /// <summary>
        /// Adds or updates multiple KPI objects.
        /// </summary>
        /// <param name="kpiObjects">List of KPIs to add/update.</param>
        /// <returns>The IDs of the KPI objects that were added/updated.</returns>
        public IList<Guid> Save(IList<IDalKpi> kpiObjects)
        {
            return SaveHelper(_repository.Service, kpiObjects);
        }

        private IList<Guid> SaveHelper(IRepository repo, IList<IDalKpi> kpiObjects)
        {
            var ids = new List<Guid>();

            foreach (var kpiObject in kpiObjects)
            {
                var kpi = repo.GetById(kpiObject.Id) as DalKpi;
                Guid id;

                // if a test doesn't exist, add it to the db
                if (kpi == null)
                {
                    repo.Add(kpiObject);
                    id = kpiObject.Id;
                }
                else
                {
                    kpi.ClassName = kpiObject.ClassName;
                    kpi.Properties = kpiObject.Properties;
                    id = kpi.Id;
                }

                ids.Add(id);
            }

            repo.SaveChanges();
            
            return ids;
        }

        public long GetDatabaseVersion(string schema, string contextKey)
        {
            return GetDatabaseVersionHelper(_repository.Service, contextKey);
        }

        private long GetDatabaseVersionHelper(IRepository repo, string contextKey)
        {
            var lastMigration = repo.GetDatabaseVersion(contextKey);

            // we are only interested in the numerical part of the key (i.e. 201609091719244_Initial)
            var version = lastMigration.Split('_')[0];

            return Convert.ToInt64(version);
        }

        [ExcludeFromCodeCoverage]
        private static bool HasTableNamed(BaseRepository repository, string table, string schema = "dbo")
        {
            string sql = @"SELECT CASE WHEN EXISTS
            (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA=@p0 AND TABLE_NAME=@p1) THEN 1 ELSE 0 END";

            using (var command = repository.DatabaseContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                command.CommandType = CommandType.Text;

                command.Parameters.Add(new SqlParameter("@p0", "dbo"));
                command.Parameters.Add(new SqlParameter("@p1", "Courses"));
                repository.DatabaseContext.Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        return (int)result[0] == 1;
                    }
                }
            }
            return false;
        }
    }
}
