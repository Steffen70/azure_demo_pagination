using DevExpress.XtraGrid.Columns;
using Newtonsoft.Json;
using SPPaginationDemo.Filtration;
using System.ComponentModel;
using System.Dynamic;
using System.Net.Http;
using System.Text;
#pragma warning disable CS4014

namespace SPPaginatedGridControl
{
    public class PaginatedBindingList<T> : BindingList<T>
    {
        private readonly HttpClient _client;
        private readonly FiltrationParams _filtrationParams;
        private int _totalCount;

        private string _endpointUrl = null!;
        public string EndpointUrl
        {
            get => _endpointUrl;
            set
            {
                _endpointUrl = value;
                // Trigger the initial data fetch when the EndpointUrl is set
                FetchDataFromServer();
            }
        }

        public PaginatedBindingList(HttpClient client)
        {
            _client = client;
            //_filtrationParams = new FiltrationParams { PageSize = 20, CurrentPage = 1 };
        }

        protected override bool SupportsSearchingCore => true;

        protected override int FindCore(PropertyDescriptor prop, object key) => FindCoreAsync(prop, key).Result;

        protected async Task<int> FindCoreAsync(PropertyDescriptor prop, object key)
        {
            var index = this
                .Select((item, index) => new { item, index })
                .FirstOrDefault(x => prop.GetValue(x.item)!.Equals(key))?.index;

            if (index != null)
                return index.Value;

            // The item wasn't found in the current list; try to fetch it from the server.
            await FetchDataFromServer();

            // Call this method again to search in the new data.
            return await FindCoreAsync(prop, key);
        }

        protected override object? AddNewCore()
        {
            FetchDataFromServer();
            return base.AddNewCore();
        }

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            base.OnListChanged(e);

            // If the last item is accessed and we have more data on the server, fetch it.
            if (e.ListChangedType == ListChangedType.ItemChanged && e.NewIndex == Count - 1 && _totalCount > Count)
                FetchDataFromServer();
        }


        private volatile int _isFetchingData;
        private async Task FetchDataFromServer(bool incrementCallCounter = true)
        {
            if (_isFetchingData > 0)
                // We are already fetching data from the server.
                return;

            if (incrementCallCounter)
                _isFetchingData = _isFetchingData + 1;

            if (_filtrationParams.CurrentPage * _filtrationParams.PageSize >= _totalCount && _totalCount != 0)
                // We have already fetched all data from the server.
                return;

            // Fetch data from server
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(EndpointUrl),
                Content = new StringContent(JsonConvert.SerializeObject(_filtrationParams), Encoding.UTF8,
                    "application/json")
            };

            try
            {
                var response = await _client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    // Optionally, handle the case when the request is not successful.
                    return;

                // Read the Filtration header
                if (response.Headers.TryGetValues("Filtration", out var headerValues))
                {
                    var header = JsonConvert.DeserializeObject<FiltrationHeader>(headerValues.First());

                    // Update total count
                    _totalCount = header!.TotalItems;
                }

                // Read and append the fetched data items.
                var contentString = await response.Content.ReadAsStringAsync();
                var fetchedData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(contentString);

                foreach (var item in fetchedData!)
                {
                    dynamic expando = new ExpandoObject();
                    var dictionaryExpando = (IDictionary<string, object>)expando;

                    foreach (var keyValuePair in item) dictionaryExpando.Add(keyValuePair);

                    Add(expando);
                }

                // Increase page number for the next fetch.
                _filtrationParams.CurrentPage++;
            }
            catch (Exception ex)
            {
                // Handle or log the exception
            }

            _isFetchingData = _isFetchingData - 1;

            if (_isFetchingData <= 0) return;

            FetchDataFromServer(false);
        }
    }
}
