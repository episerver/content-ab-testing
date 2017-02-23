﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Migrations.History;
using System.Linq;
using EPiServer.Marketing.KPI.Dal;
using EPiServer.Marketing.KPI.Dal.Model;
using EPiServer.Marketing.KPI.Exceptions;

namespace EPiServer.Marketing.KPI.DataAccess
{
    internal class KpiDataAccess : IKpiDataAccess
    {
        internal IRepository _repository;
        internal bool _UseEntityFramework;

        public KpiDataAccess()
        {
            _UseEntityFramework = true;

            using (var dbContext = new DatabaseContext())
            {
                var repository = new BaseRepository(dbContext);

                if (!HasTableNamed(repository, "tblKeyPerformaceIndicator"))
                {
                    // the sql scripts need to be run!
                    throw new DatabaseDoesNotExistException();
                }
            }
            // TODO : Load repository from service locator.
        }
        internal KpiDataAccess(IRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Deletes KPI object from the db.
        /// </summary>
        /// <param name="kpiId">Id of the KPI to delete.</param>
        public void Delete(Guid kpiId)
        {
            if (_UseEntityFramework)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var repository = new BaseRepository(dbContext);
                    DeleteHelper(repository, kpiId);
                }
            }
            else
            {
                DeleteHelper(_repository, kpiId);
            }
        }

        private void DeleteHelper(IRepository repo, Guid kpiId)
        {
            repo.DeleteKpi(kpiId);
            repo.SaveChanges();
        }

        /// <summary>
        /// Returns a KPI object based on its Id.
        /// </summary>
        /// <param name="kpiId">Id of the KPI to retrieve.</param>
        /// <returns>KPI object.</returns>
        public IDalKpi Get(Guid kpiId)
        {
            IDalKpi kpi;

            if (_UseEntityFramework)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var repository = new BaseRepository(dbContext);
                    kpi = GetHelper(repository, kpiId);
                }
            }
            else
            {
                kpi = GetHelper(_repository, kpiId);
            }

            return kpi;
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
            List<IDalKpi> kpis;

            if (_UseEntityFramework)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var repository = new BaseRepository(dbContext);
                    kpis = GetKpiListHelper(repository);
                }
            }
            else
            {
                kpis = GetKpiListHelper(_repository);
            }

            return kpis;
        }

        private List<IDalKpi> GetKpiListHelper(IRepository repo)
        {
            return repo.GetAll().ToList();
        }

        /// <summary>
        /// Adds or updates a KPI object.
        /// </summary>
        /// <param name="kpiObject">Id of the KPI to add/update.</param>
        /// <returns>The Id of the KPI object that was added/updated.</returns>
        public Guid Save(IDalKpi kpiObject)
        {
            Guid id;

            if (_UseEntityFramework)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var repository = new BaseRepository(dbContext);
                    id = SaveHelper(repository, kpiObject);
                }
            }
            else
            {
                id = SaveHelper(_repository, kpiObject);
            }

            return id;
        }


        public Guid SaveHelper(IRepository repo, IDalKpi kpiObject)
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

            repo.SaveChanges();

            return id;
        }

        public long GetDatabaseVersion(DbConnection dbConnection, string schema, string contextKey)
        {
            long version = 0;

            if (_UseEntityFramework)
            {
                using (var historyContext = new HistoryContext(dbConnection, schema))
                {
                    var repository = new BaseRepository(historyContext);
                    version = GetDatabaseVersionHelper(repository, contextKey);
                }
            }
            else
            {
                version = GetDatabaseVersionHelper(_repository, contextKey);
            }

            return version;
        }

        private long GetDatabaseVersionHelper(IRepository repo, string contextKey)
        {
            var lastMigration = repo.GetDatabaseVersion(contextKey);

            // we are only interested in the numerical part of the key (i.e. 201609091719244_Initial)
            var version = lastMigration.Split('_')[0];

            return Convert.ToInt64(version);
        }

        private static bool HasTableNamed(BaseRepository repository, string table, string schema = "dbo")
        {
            string sql = @"SELECT CASE WHEN EXISTS
            (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA=@p0 AND TABLE_NAME=@p1) THEN 1 ELSE 0 END";

            return repository.DatabaseContext.Database.SqlQuery<int>(sql, schema, table).Single() == 1;
        }
    }
}