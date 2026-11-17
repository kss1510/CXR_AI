using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class KnowledgeSearchWindow : EditorWindow
{
    private string searchQuery = "";
    private Vector2 resultsScrollPosition;
    private List<Entry> searchResults = new List<Entry>();
    private string ragQuestion = "";
    private string ragAnswer = "";
    // COLLAPSIBLE STATE
    private Dictionary<int, bool> expandedEntries =
        new Dictionary<int, bool>();
    private int currentlyEditingID = -1;

    // FILTER SYSTEM
    private int filterIndex = 0;

    private string[] filters =
    {
        "All",
        "Title",
        "Category",
        "Tags",
        "Problem",
        "Solution"
    };
    private int sortIndex = 0;

    private string[] sortOptions =
    {
    "Relevance",
    "Newest",
    "Oldest",
    "Title"
    };
    private string filePath;
    private Vector2 mainScrollPosition;
    private GUIStyle richLabelStyle;
    private bool isSearching = false;
    private string backendStatus = "Connected";
    private string backendErrorMessage = "";
    private string lastSynced = "Never";
    [MenuItem("Tools/Knowledge Search")]
    public static void ShowWindow()
    {
        var window =
            GetWindow<KnowledgeSearchWindow>(
                "Knowledge Search"
            );

        window.minSize =
            new Vector2(700, 700);
    }

    private void OnEnable()
    {
        filePath = Path.Combine(
            Application.dataPath,
            "knowledge.json"
        );
    }

    private void OnGUI()
    {
        mainScrollPosition =
            EditorGUILayout.BeginScrollView(
                mainScrollPosition
            );
        GUILayout.Space(10);
        // SAFE GUI STYLE INIT
        if (richLabelStyle == null)
        {
            richLabelStyle =
                new GUIStyle(EditorStyles.label);

            richLabelStyle.richText = true;

            richLabelStyle.wordWrap = true;
        }

        GUILayout.Label(
            "Knowledge Search",
            EditorStyles.boldLabel
        );

        GUILayout.Space(10);

        // SEARCH FIELD
        GUILayout.Label(
            "Search",
            EditorStyles.boldLabel
        );

        searchQuery = EditorGUILayout.TextField(
            searchQuery,
            GUILayout.Height(32),
            GUILayout.ExpandWidth(true)
        );
        GUILayout.Space(10);

        GUILayout.Box(
            "",
            GUILayout.ExpandWidth(true),
            GUILayout.Height(1)
        );

        GUILayout.Space(5);
        GUILayout.Space(10);


        GUILayout.Space(10);

        if (isSearching)
        {
            EditorGUILayout.HelpBox(
                "Searching Backend...",
                MessageType.Info
            );
        }
        GUILayout.BeginHorizontal("box");

        GUILayout.Label(
            $"Backend: {backendStatus}",
            GUILayout.Width(180)
        );

        GUILayout.FlexibleSpace();

        GUILayout.Label(
            $"Last Sync: {lastSynced}"
        );

        GUILayout.EndHorizontal();
        GUILayout.Space(5);

        GUILayout.Box(
            "",
            GUILayout.ExpandWidth(true),
            GUILayout.Height(1)
        );

        GUILayout.Space(5);
        if (!string.IsNullOrEmpty(
            backendErrorMessage))
        {
            EditorGUILayout.HelpBox(
                backendErrorMessage,
                MessageType.Error
            );
        }
        GUILayout.BeginHorizontal("box");

        GUILayout.BeginVertical();
        GUILayout.Label("Filter");
        filterIndex =
            EditorGUILayout.Popup(
                filterIndex,
                filters
            );
        GUILayout.EndVertical();

        GUILayout.Space(20);

        GUILayout.BeginVertical();
        GUILayout.Label("Sort");
        sortIndex =
            EditorGUILayout.Popup(
                sortIndex,
                sortOptions
            );
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        GUILayout.Space(5);

        GUILayout.Box(
            "",
            GUILayout.ExpandWidth(true),
            GUILayout.Height(1)
        );

        GUILayout.Space(5);
        GUILayout.Space(10);

        // =========================
        // BUTTONS
        // =========================
        GUILayout.BeginHorizontal(
            GUILayout.ExpandWidth(true)
        );
        GUILayout.Space(5);
        if (GUILayout.Button(
            "Search",
            GUILayout.Height(35),
            GUILayout.ExpandWidth(true)
        ))
        {
            SearchKnowledge();
        }
        GUILayout.Space(5);
        if (GUILayout.Button(
            "Clear",
            GUILayout.Height(35),
            GUILayout.ExpandWidth(true)
        ))
        {
            searchQuery = "";
            searchResults.Clear();
            backendErrorMessage = "";
            backendStatus = "Connected";
            ragQuestion = "";
            ragAnswer = "";
        }

        GUILayout.EndHorizontal();

        

        GUILayout.Space(10);

        GUILayout.BeginHorizontal(
            GUILayout.ExpandWidth(true)
        );

        if (GUILayout.Button(
            "Export TXT",
            GUILayout.Height(30),
            GUILayout.ExpandWidth(true)
        ))
        {
            ExportResults();
        }

        if (GUILayout.Button(
            "Export CSV",
            GUILayout.Height(30),
            GUILayout.ExpandWidth(true)
        ))
        {
            ExportCSV();
        }

        GUILayout.EndHorizontal();


        // =========================
        // RESULT COUNT
        // =========================
        GUILayout.Space(10);

        GUILayout.Box(
            "",
            GUILayout.ExpandWidth(true),
            GUILayout.Height(1)
        );

        GUILayout.Space(10);

        GUILayout.Label(
            $"Knowledge Results ({searchResults.Count})",
            EditorStyles.boldLabel
        );
        GUILayout.Space(5);

        GUILayout.Box(
            "",
            GUILayout.ExpandWidth(true),
            GUILayout.Height(1)
        );

        GUILayout.Space(5);

        // =========================    
        // RAG ANSWER SECTION
        // =========================
        if (!string.IsNullOrEmpty(ragAnswer))
        {
            GUILayout.Space(10);

            GUILayout.Label(
                "RAG Response",
                EditorStyles.boldLabel
            );

            GUILayout.BeginVertical("box");

            GUILayout.Label(
                $"Question: {ragQuestion}",
                EditorStyles.boldLabel
            );

            GUILayout.Space(5);

            GUILayout.Label(
                ragAnswer,
                EditorStyles.wordWrappedLabel
            );

            GUILayout.EndVertical();

            GUILayout.Space(10);
        }

        GUILayout.Space(5);

        // =========================
        // RESULTS SECTION
        // =========================

        if (searchResults.Count > 0)
        {
            GUILayout.Space(5);

            GUILayout.Label(
                "Most Relevant Results",
                EditorStyles.boldLabel
            );

            GUILayout.Box(
                "",
                GUILayout.ExpandWidth(true),
                GUILayout.Height(1)
            );

            GUILayout.Space(5);
        }

        GUILayout.BeginVertical(
            "box",
            GUILayout.ExpandWidth(true)
        );

        if (searchResults.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No matching knowledge entries found.",
                MessageType.Info
            );
        }
        else
        {
            resultsScrollPosition =
                EditorGUILayout.BeginScrollView(
                    resultsScrollPosition,
                    GUILayout.Height(400)
                );

            // Iterate over a snapshot because an asynchronous delete can
            // update searchResults while this GUI is being drawn.
            foreach (var entry in searchResults.ToArray())
            {
                DrawEntry(entry);
            }

            EditorGUILayout.EndScrollView();
        }
        GUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }
        void GenerateSearchRequestPayload()
    {
        SearchRequest request =
            new SearchRequest();

        request.query =
            searchQuery;

        request.filter =
            filters[filterIndex];

        request.sort =
            sortOptions[sortIndex];

        //request.timestamp =
        //    System.DateTime.Now.ToString(
        //        "yyyy-MM-dd HH:mm:ss"
        //    );

        string json =
            JsonUtility.ToJson(
                request,
                true
            );

        Debug.Log(
            "SEARCH REQUEST JSON:\n" +
            json
        );
    }

    async void SendSearchRequestToBackend()
    {
        isSearching = true;
        backendErrorMessage = "";
        Repaint();
        SearchRequest request =
            new SearchRequest();

        request.query = searchQuery;

        request.filter =
            filters[filterIndex];

        request.sort =
            sortOptions[sortIndex];

        //request.timestamp =
        //    System.DateTime.Now.ToString(
        //        "yyyy-MM-dd HH:mm:ss"
        //    );

        string json =
            JsonUtility.ToJson(
                request,
                true
            );


        byte[] bodyRaw =
            Encoding.UTF8.GetBytes(json);

        UnityWebRequest webRequest =
            new UnityWebRequest(
                "http://127.0.0.1:8000/search",
                "POST"
            );

        webRequest.uploadHandler =
            new UploadHandlerRaw(bodyRaw);

        webRequest.downloadHandler =
            new DownloadHandlerBuffer();

        webRequest.SetRequestHeader(
            "Content-Type",
            "application/json"
        );

        var operation =
            webRequest.SendWebRequest();

        while (!operation.isDone)
        {
            await System.Threading.Tasks.Task.Yield();
        }

        if (webRequest.result ==
            UnityWebRequest.Result.Success)
        {
            string responseJson =
                webRequest.downloadHandler.text;

            Debug.Log(
                "BACKEND RESPONSE:\n" +
                responseJson
            );

            SearchResponse parsedResponse =
                JsonUtility.FromJson<SearchResponse>(
                    responseJson
            );

            if (parsedResponse != null &&
    parsedResponse.results != null)
            {
                searchResults.Clear();

                foreach (var result in parsedResponse.results)
                {
                    searchResults.Add(result);
                }

                if (parsedResponse.rag_response != null)
                {
                    ragQuestion =
                        parsedResponse.rag_response.question;

                    ragAnswer =
                        parsedResponse.rag_response.answer;
                }
                backendStatus = "Connected";

                lastSynced = "-";
                    DateTime.Now.ToString("HH:mm:ss");
                Repaint();

                Debug.Log(
                    $"Backend Returned: {searchResults.Count} result(s)"
                );
            }


        }
        else
        {
            Debug.LogError(
                "BACKEND ERROR:\n" +
                "Status Code: " +
                webRequest.responseCode +
                "\nResponse:\n" +
                webRequest.downloadHandler.text
            );
            backendStatus = "Offline";
            backendErrorMessage =
                "Unable to connect to AI Backend.";
        }
        isSearching = false;
        Repaint();
        
    }
    // =========================================
    // SEARCH LOGIC
    // =========================================
    void SearchKnowledge()
    {
        searchResults.Clear();

        SendSearchRequestToBackend();
    }

    // =========================================
    // DRAW ENTRY
    // =========================================
    void DrawEntry(Entry entry)
    {
        GUILayout.BeginVertical("box");

        // INITIALIZE STATE
        if (!expandedEntries.ContainsKey(entry.id))
        {
            expandedEntries[entry.id] = false;
        }

        bool expanded =
            expandedEntries[entry.id];

        // HEADER
        string arrow =
            expanded ? "▼" : "▶";

        GUIStyle headerStyle =
            new GUIStyle(EditorStyles.boldLabel);

        headerStyle.richText = true;

        if (GUILayout.Button(
            $"{arrow} {HighlightMatch(entry.title)}",
            headerStyle
        ))
        {
            expandedEntries[entry.id] =
                !expanded;
        }

        // EXPANDED CONTENT
        if (expandedEntries[entry.id])
        {
            bool isEditing =
                currentlyEditingID == entry.id;

            GUILayout.Space(5);

            // =====================================
            // EDIT MODE
            // =====================================
            if (isEditing)
            {
                GUILayout.Label("Title");
                entry.title = EditorGUILayout.TextField(
                    entry.title
                );

                GUILayout.Label("Category");
                entry.category = EditorGUILayout.TextField(
                    entry.category
                );

                GUILayout.Label("Tags");

                string tagsString =
                    string.Join(",", entry.tags);

                tagsString =
                    EditorGUILayout.TextField(tagsString);

                entry.tags =
                    new List<string>(
                        tagsString.Split(',')
                    );

                // DYNAMIC HEIGHTS
                float problemHeight =
                    Mathf.Max(
                        60,
                        EditorStyles.textArea.CalcHeight(
                            new GUIContent(entry.problem),
                            position.width - 40
                        )
                    );

                float solutionHeight =
                    Mathf.Max(
                        60,
                        EditorStyles.textArea.CalcHeight(
                            new GUIContent(entry.solution),
                            position.width - 40
                        )
                    );

                // PROBLEM FIELD
                GUILayout.Label("Problem");

                entry.problem =
                    EditorGUILayout.TextArea(
                        entry.problem,
                        GUILayout.Height(problemHeight)
                    );

                // SOLUTION FIELD
                GUILayout.Label("Solution");

                entry.solution =
                    EditorGUILayout.TextArea(
                        entry.solution,
                        GUILayout.Height(solutionHeight)
                    );  
            }

            // =====================================
            // NORMAL VIEW MODE
            // =====================================
            else
            {
                GUILayout.Label(
                    $"Category: {HighlightMatch(entry.category)}",
                    richLabelStyle
                );

                // TAGS LABEL
                GUILayout.Label("Tags");

                // SPLIT TAGS
                List<string> splitTags =
                    entry.tags;

                // TAG BUTTON ROW
                GUILayout.BeginHorizontal();

                foreach (string rawTag in splitTags)
                {
                    string tag = rawTag.Trim();

                    if (GUILayout.Button(
                        HighlightMatch(tag),
                        richLabelStyle,
                        GUILayout.Height(25)
                    ))
                    {
                        searchQuery = tag;

                        // TAG FILTER INDEX
                        filterIndex = 4;

                        SearchKnowledge();

                        Debug.Log(
                            $"Quick Filter Tag: {tag}"
                        );
                    }
                }

                GUILayout.EndHorizontal();

                GUILayout.Label(
                    $"Problem: {HighlightMatch(entry.problem)}",
                    richLabelStyle
                );

                GUILayout.Label(
                    $"Solution: {HighlightMatch(entry.solution)}",
                    richLabelStyle
                );
            }

            GUILayout.Label(
                $"Created: {entry.createdAt}"
            );
            

            GUILayout.Space(10);

            // =====================================
            // ACTION BUTTONS
            // =====================================
            GUILayout.BeginHorizontal();

            // =========================
            // EDIT BUTTON
            // =========================
            if (!isEditing)
            {
                if (GUILayout.Button("Edit"))
                {
                    currentlyEditingID = entry.id;

                    Debug.Log(
                        $"Editing Mode Enabled: {entry.id}"
                    );
                }
            }

            // =========================
            // SAVE BUTTON
            // =========================
            if (isEditing)
            {
                if (GUILayout.Button("Save Changes"))
                {
                    SaveEditedEntry(entry);

                    currentlyEditingID = -1;

                    Debug.Log(
                        $"Saved Changes: {entry.id}"
                    );
                }
            }
            
            // =========================
            // DELETE BUTTON
            // =========================
            if (GUILayout.Button("Delete"))
            {
                DeleteEntry(entry.id);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        GUILayout.Space(8);
    }

    async void DeleteEntry(int entryID)
    {
        bool confirm = EditorUtility.DisplayDialog(
            "Delete Knowledge Entry",
            $"Are you sure you want to delete entry {entryID}?",
            "Delete",
            "Cancel"
        );

        if (!confirm)
        {
            return;
        }

        string url =
            $"http://127.0.0.1:8000/query/{entryID}";

        // Ask the backend to delete the database record first.
        using (UnityWebRequest webRequest = UnityWebRequest.Delete(url))
        {
            webRequest.downloadHandler =
                new DownloadHandlerBuffer();

            try
            {
                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                {
                    await System.Threading.Tasks.Task.Yield();
                }

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(
                        "DELETE FAILED:\n" +
                        "Status Code: " + webRequest.responseCode +
                        "\nResponse: " +
                        webRequest.downloadHandler.text
                    );

                    backendStatus = "Offline";
                    backendErrorMessage =
                        "Unable to delete entry from backend. The entry remains in the results.";

                    Repaint();
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "DELETE REQUEST ERROR: " + ex.Message
                );

                backendStatus = "Offline";
                backendErrorMessage =
                    "An error occurred while deleting the entry from the backend.";

                Repaint();
                return;
            }
        }

        // The backend confirmed deletion. Update the local JSON copy if it exists.
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);

                Wrapper data =
                    JsonUtility.FromJson<Wrapper>(json);

                if (data != null && data.entries != null)
                {
                    data.entries.RemoveAll(
                        entry => entry.id == entryID
                    );

                    string updatedJson =
                        JsonUtility.ToJson(data, true);

                    File.WriteAllText(filePath, updatedJson);
                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.LogWarning(
                        "Backend deletion succeeded, but local knowledge.json " +
                        "does not contain a valid entries list."
                    );
                }
            }
            catch (Exception ex)
            {
                // Do not report the backend deletion as failed because the
                // database record has already been deleted successfully.
                Debug.LogWarning(
                    "Backend deletion succeeded, but local knowledge.json " +
                    "could not be updated: " + ex.Message
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "Backend deletion succeeded. Local knowledge.json was not found; " +
                "the local file was left unchanged."
            );
        }

        // Update the UI only after the backend confirms the deletion.
        searchResults.RemoveAll(
            entry => entry.id == entryID
        );

        expandedEntries.Remove(entryID);

        if (currentlyEditingID == entryID)
        {
            currentlyEditingID = -1;
        }

        backendStatus = "Connected";
        backendErrorMessage = "";

        Debug.Log(
            $"Successfully deleted Entry: {entryID} from the backend."
        );

        Repaint();
    }
    void SaveEditedEntry(Entry updatedEntry)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError(
                "knowledge.json not found"
            );
            return;
        }

        string json =
            File.ReadAllText(filePath);

        Wrapper data =
            JsonUtility.FromJson<Wrapper>(json);

        if (data == null ||
            data.entries == null)
        {
            Debug.LogError(
                "No entries found"
            );
            return;
        }

        // FIND ENTRY
        for (int i = 0; i < data.entries.Count; i++)
        {
            if (data.entries[i].id ==
                updatedEntry.id)
            {
                data.entries[i] = updatedEntry;
                break;
            }
        }

        // SAVE UPDATED JSON
        string updatedJson =
            JsonUtility.ToJson(data, true);

        File.WriteAllText(
            filePath,
            updatedJson
        );

        AssetDatabase.Refresh();
    }
    string HighlightMatch(string text)
    {
        if (string.IsNullOrEmpty(searchQuery))
        {
            return text;
        }

        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        string highlightedText = text;

        string lowerText =
            text.ToLower();

        string lowerQuery =
            searchQuery.ToLower();

        int startIndex = 0;

        while (true)
        {
            int matchIndex =
                lowerText.IndexOf(
                    lowerQuery,
                    startIndex
                );

            if (matchIndex < 0)
            {
                break;
            }

            string originalMatch =
                highlightedText.Substring(
                    matchIndex,
                    searchQuery.Length
                );

            string highlightedMatch =
                $"<color=yellow><b>{originalMatch}</b></color>";

            highlightedText =
                highlightedText.Remove(
                    matchIndex,
                    searchQuery.Length
                );

            highlightedText =
                highlightedText.Insert(
                    matchIndex,
                    highlightedMatch
                );

            // UPDATE LOWER TEXT
            lowerText =
                highlightedText.ToLower();

            startIndex =
                matchIndex +
                highlightedMatch.Length;
        }

        return highlightedText;
    }
    void ExportResults()
    {
        if (searchResults.Count == 0)
        {
            Debug.Log("No search results to export");
            return;
        }

        string exportPath =
            Path.Combine(
                Application.dataPath,
                "knowledge_export.txt"
            );

        List<string> lines =
            new List<string>();

        foreach (var entry in searchResults)
        {
            lines.Add(
                "=================================="
            );
            lines.Add($"Title: {entry.title}");

            lines.Add($"Category: {entry.category}");

            lines.Add($"Tags: {string.Join(",", entry.tags)}");

            lines.Add($"Problem: {entry.problem}");

            lines.Add($"Solution: {entry.solution}");

            

            lines.Add(
                $"Created: {entry.createdAt}"
            );

            lines.Add("");
        }

        File.WriteAllLines(exportPath, lines);

        AssetDatabase.Refresh();

        Debug.Log(
            $"Exported Results: {exportPath}"
        );
    }

    BackendEntry ConvertToBackendEntry(
    Entry entry
)
    {
        BackendEntry backendEntry =
            new BackendEntry();


        backendEntry.title =
            entry.title;

        backendEntry.category =
            entry.category;

        backendEntry.problem =
            entry.problem;

        backendEntry.solution =
            entry.solution;

        backendEntry.codeSnippet = "";

        // CONVERT TAGS STRING → LIST
        backendEntry.tags =
            new List<string>();

        backendEntry.tags =
            new List<string>(entry.tags);

        return backendEntry;
    }

    void GenerateBackendPayload(
    Entry entry
    )
    {
        BackendEntry backendEntry =
            ConvertToBackendEntry(entry);

        string json =
            JsonUtility.ToJson(
                backendEntry,
                true
            );

        Debug.Log(
            "BACKEND PAYLOAD:\n" +
            json
        );
    }

    void ExportCSV()
    {
        if (searchResults.Count == 0)
        {
            Debug.Log(
                "No search results to export"
            );

            return;
        }

        string exportPath =
            Path.Combine(
                Application.dataPath,
                "knowledge_export.csv"
            );

        List<string> lines =
            new List<string>();

        // CSV HEADER
        lines.Add(
            "Title,Category,Tags,Problem,Solution,CreatedAt"
        );

        foreach (var entry in searchResults)
        {
            string line =
                $"\"{entry.title}\"," +
                $"\"{entry.category}\"," +
                $"\"{string.Join(",", entry.tags)}\"," +
                $"\"{entry.problem}\"," +
                $"\"{entry.solution}\"," +
                $"\"{entry.createdAt}\"";

            lines.Add(line);
        }

        File.WriteAllLines(exportPath, lines);

        AssetDatabase.Refresh();

        Debug.Log(
            $"CSV Exported: {exportPath}"
        );
    }
    // =========================================
    // DATA MODEL
    // =========================================
    [System.Serializable]
    public class Entry
    {
        public int id;

        public string title;

        public string category;

        public List<string> tags;

        public string problem;

        public string solution;

        public string createdAt;
        [System.NonSerialized]
        public int relevanceScore;
        [System.NonSerialized]
        public List<string> matchedFields =
        new List<string>();
        public string codeSnippet;

        public string gen_answer;
    }

    [System.Serializable]
    public class BackendEntry
    {
        public string title;

        public string category;

        public string problem;

        public string solution;

        public string codeSnippet;

        public List<string> tags;
    }

    [System.Serializable]
    public class SearchRequest
    {
        public string query;

        public string filter;   

        public string sort;

        //public string timestamp;
    }

    [System.Serializable]
    public class BackendEntryList
    {
        public List<Entry> entries;
    }

    [System.Serializable]
    public class Wrapper
    {
        public List<Entry> entries;
    }
    [System.Serializable]
    public class RAGResponse
    {
        public string question;

        public string answer;
    }
    [System.Serializable]
    public class SearchResponse
    {
        public List<Entry> results;

        public RAGResponse rag_response;
    }
}