from flask import Flask, request, jsonify, render_template_string
import os
import requests
from bs4 import BeautifulSoup
from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain.embeddings.openai import OpenAIEmbeddings
from langchain.vectorstores import Chroma
from langchain.prompts import ChatPromptTemplate
from langchain.chat_models import ChatOpenAI

# Configuration

app = Flask(__name__)

def download_simpy_docs(base_url):
    """Download and process SimPy documentation"""
    response = requests.get(base_url)
    if response.status_code != 200:
        raise Exception(f"Error accessing {base_url}: {response.status_code}")
    soup = BeautifulSoup(response.text, "html.parser")
    return "\n".join(p.get_text() for p in soup.find_all("p"))

def rag(user_query):
    """RAG function to generate SimPy code"""
    DOC_URL = "https://simpy.readthedocs.io/en/latest/"
    CHROMA_PATH = "simpy_docs"
    
    # Process documentation
    doc_text = download_simpy_docs(DOC_URL)
    text_splitter = RecursiveCharacterTextSplitter(chunk_size=1000, chunk_overlap=200)
    chunks = text_splitter.split_text(doc_text)
    
    # Create embeddings
    embeddings = OpenAIEmbeddings(openai_api_key=os.environ['OPENAI_API_KEY'])
    db = Chroma.from_texts(chunks, embeddings, persist_directory=CHROMA_PATH)
    docs = db.similarity_search_with_score(user_query, k=5)
    context = "\n\n".join([doc.page_content for doc, _ in docs])

    # Generate response
    prompt_template = ChatPromptTemplate.from_template("""
    You are a SimPy expert. Generate Python code for:
    {query}
    Use this context: {context}
    Respond ONLY with properly formatted Python code.
    Include all necessary imports.
    Use 4-space indentation.
    """)
    
    response = ChatOpenAI().predict(prompt_template.format(
        query=user_query,
        context=context
    ))
    
    return {
        "answer": response,
        "context": context
    }

@app.route('/')
def home():
    """Main page with pre-loaded test questions and chat interface"""
    test_questions = [
        "Create a queue simulation using SimPy",
        "Create a traffic light (semaphore) simulation using SimPy",
        "Create a manufacturing machine construction simulation using SimPy"
    ]
    
    # Generate test messages HTML
    messages_html = ""
    for question in test_questions:
        result = rag(question)
        messages_html += f"""
        <div class="message user">{question}</div>
        <div class="message bot"><pre>{result['answer']}</pre></div>
        """
    
    return render_template_string('''
<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>SimPy Simulator</title>
    <style>
        .navbar {
            background-color: white;
            padding: 10px 0;
            text-align: center;
            border-bottom: 1px solid #ddd;
        }
        .navbar button {
            background-color: white;
            color: black;
            border: none;
            padding: 10px 20px;
            margin: 0 10px;
            font-size: 16px;
            cursor: pointer;
            transition: background-color 0.3s ease;
        }
        .navbar button:hover {
            background-color: #f0f0f0;
        }
        body {
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 0;
            display: flex;
            flex-direction: column;
            height: 100vh;
        }
        .chat-container {
            flex: 1;
            padding: 20px;
            overflow-y: auto;
            background-color: #f9f9f9;
        }
        .message {
            margin-bottom: 15px;
            padding: 10px;
            border-radius: 10px;
            max-width: 70%;
        }
        .message.user {
            background-color: #007bff;
            color: white;
            margin-left: auto;
        }
        .message.bot {
            background-color: #e9ecef;
            color: black;
            margin-right: auto;
        }
        .chat-input {
            padding: 10px;
            background-color: #fff;
            border-top: 1px solid #ddd;
            display: flex;
        }
        .chat-input input {
            flex: 1;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 5px;
            font-size: 16px;
        }
        .chat-input button {
            padding: 10px 20px;
            margin-left: 10px;
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            font-size: 16px;
        }
        .chat-input button:hover {
            background-color: #0056b3;
        }
        pre {
            white-space: pre-wrap;
            margin: 0;
            font-family: monospace;
        }
    </style>
</head>
<body>
    <div class="navbar">
        <button onclick="openEditor()">Editor</button>
        <button onclick="window.location.href='documentacao.html'">Documentation</button>
        <button>Code</button>
        <button onclick="window.location.href='chat.html'">Chat</button>
    </div>

    <div class="chat-container" id="chat-messages">
        {{ messages_html|safe }}
    </div>

    <div class="chat-input">
        <input type="text" id="chat-input" placeholder="Type your message..." autofocus>
        <button onclick="sendMessage()">Send</button>
    </div>

    <script>
        async function sendMessage() {
            const input = document.getElementById('chat-input');
            const message = input.value.trim();
            if (!message) return;

            // Add user message
            addMessage(message, 'user');
            input.value = '';
            
            try {
                // Get bot response
                const response = await fetch('/chat', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ message: message })
                });
                const data = await response.json();
                addMessage(data.response, 'bot');
            } catch (error) {
                addMessage('Error: Could not get response', 'bot');
                console.error('Error:', error);
            }
        }

        function addMessage(content, sender) {
            const container = document.getElementById('chat-messages');
            const msgDiv = document.createElement('div');
            msgDiv.className = `message ${sender}`;
            
            if (content.includes('\n')) {
                const pre = document.createElement('pre');
                pre.textContent = content;
                msgDiv.appendChild(pre);
            } else {
                msgDiv.textContent = content;
            }
            
            container.appendChild(msgDiv);
            container.scrollTop = container.scrollHeight;
        }

        // Send message on Enter key
        document.getElementById('chat-input').addEventListener('keypress', (e) => {
            if (e.key === 'Enter') sendMessage();
        });
    </script>
</body>
</html>
    ''', messages_html=messages_html)

@app.route('/chat', methods=['POST'])
def chat():
    """Endpoint for chat messages"""
    user_input = request.json.get('message')
    result = rag(user_input)
    return jsonify({"response": result['answer']})

if __name__ == "__main__":
    app.run(host='...', port=...)
