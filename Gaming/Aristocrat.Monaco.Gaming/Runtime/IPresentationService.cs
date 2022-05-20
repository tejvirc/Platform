namespace Aristocrat.Monaco.Gaming.Runtime
{
    using Client;
    using Contracts;
    using System.Collections.Generic;

    public interface IPresentationService : IClientEndpoint
    {
        void PresentOverriddenPresentation(IList<PresentationOverrideData> presentations);
    }
}
