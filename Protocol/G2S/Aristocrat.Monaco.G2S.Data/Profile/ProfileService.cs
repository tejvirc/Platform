namespace Aristocrat.Monaco.G2S.Data.Profile
{
    using Aristocrat.G2S.Client.Devices;
    using Common.Storage;
    using Model;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Default implementation of profile service.
    /// </summary>
    public class ProfileService : IProfileService
    {
        private readonly IMonacoContextFactory _contextFactory;

        private readonly IProfileDataRepository _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProfileService" /> class.
        /// </summary>
        /// <param name="contextFactory">The database context factory.</param>
        /// <param name="repository">Profile data repository instance.</param>
        public ProfileService(IMonacoContextFactory contextFactory, IProfileDataRepository repository)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc />
        public void Populate<T>(T profile)
            where T : IDevice
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var data = _repository.Get(context, profile.DeviceClass, profile.Id);

                if (data != null)
                {
                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new PrivateSetterContractResolver(),
                        ObjectCreationHandling = ObjectCreationHandling.Replace
                    };

                    JsonConvert.PopulateObject(data.Data, profile, settings);
                }
            }
        }

        /// <inheritdoc />
        public bool Exists<T>(T profile)
            where T : IDevice
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                return _repository.Get(context, profile.DeviceClass, profile.Id) != null;
            }
        }

        /// <inheritdoc />
        public IEnumerable<ProfileData> GetAll()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return _repository.GetAll(context).ToList();
            }
        }

        /// <inheritdoc />
        public void Save<T>(T profile)
            where T : IDevice
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var json = JsonConvert.SerializeObject(profile);
                var data = _repository.Get(context, profile.DeviceClass, profile.Id);

                if (data != null)
                {
                    data.Data = json;
                    _repository.Update(context, data);
                }
                else
                {
                    _repository.Add(
                        context,
                        new ProfileData { DeviceId = profile.Id, ProfileType = profile.DeviceClass, Data = json });
                }
            }
        }

        /// <inheritdoc />
        public void Delete<T>(T profile)
            where T : IDevice
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var data = _repository.Get(context, profile.DeviceClass, profile.Id);
                if (data != null)
                {
                    _repository.Delete(context, data);
                }
            }
        }
    }
}
