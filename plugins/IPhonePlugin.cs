namespace PhoneToLinux.Core
{
    /// <summary>
    /// Reprezentuje wspólny interfejs dla wszystkich wtyczek (.dnn) po stronie desktopowej.
    /// Każda nowa funkcja ładowana dynamicznie musi implementować ten kontrakt.
    /// </summary>
    public interface IPhonePlugin
    {
        /// <summary>
        /// Ścieżka endpointu obsługiwana przez wtyczkę (np. "/conversations").
        /// </summary>
        string Endpoint { get; }

        /// <summary>
        /// Wykonuje główną logikę wtyczki na podstawie przekazanych parametrów żądania.
        /// </summary>
        /// <param name="queryParams">Parametry wejściowe przekazane w adresie URL.</param>
        /// <returns>Odpowiedź w formacie JSON lub tekstowym.</returns>
        string Execute(string queryParams);
    }
}