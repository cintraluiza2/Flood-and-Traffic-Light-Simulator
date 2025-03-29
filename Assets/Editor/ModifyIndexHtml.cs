using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class PostBuildScript : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.WebGL)
        {
            string buildPath = report.summary.outputPath;
            string indexPath = Path.Combine(buildPath, "index.html");

            if (File.Exists(indexPath))
            {
                string content = File.ReadAllText(indexPath);

                string styleToAdd = @"
                    <style>

                        /* Remove margens e usa altura total da tela */
                        html, body {
                            margin: 0;
                            padding: 0;
                            width: 100%;
                            height: 100vh;
                            display: flex;
                            flex-direction: column;
                            overflow: hidden;
                        }

                        /* Estilo dos botões */
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

                        /* Efeito ao passar o mouse sobre os botões */
                        .navbar button:hover {
                            background-color: #f0f0f0;
                        }

                        /* Garante que o navbar fique fixo no topo */
                        .navbar {
                            background-color: white;
                            padding: 10px 0;
                            text-align: center;
                            border-bottom: 1px solid #ddd;
                            width: 100%;
                            position: fixed;
                            top: 0;
                            left: 0;
                            z-index: 10;
                        }

                        /* Área de conteúdo abaixo da navbar */
                        .content {
                            flex-grow: 1;
                            display: flex;
                            flex-direction: column;
                            align-items: center;
                            justify-content: flex-start;
                            padding-top: 60px; /* Garante espaço para a navbar */
                            width: 100%;
                        }

                        /* Container da Unity ocupando o espaço disponível corretamente */
                        #unity-container {
                            flex-grow: 1;
                            display: flex;
                            align-items: center;
                            justify-content: center;
                            width: 100%;
                            max-height: calc(100vh - 60px); /* Remove a altura da navbar */
                            overflow: hidden;
                        }

                        /* Ajusta o canvas da Unity para evitar cortes */
                        #unity-canvas {
                            width: 100%;
                            height: 100%;
                            object-fit: contain; /* Garante que o conteúdo não seja cortado */
                        }

                        /* Estilo do gráfico para centralizar e evitar sumiço */
                        .chart-container {
                            width: 300px;
                            max-width: 90%;
                            margin: 20px auto;
                            background: white;
                            padding: 20px;
                            border-radius: 8px;
                            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                            position: relative;
                            z-index: 5;
                        }

                        #unity-webgl-logo, {
                            display: none !important;
                        }



                    </style>
                ";

                string navbarToAdd = @"
                    <!-- Barra de navegação -->
                    <div class='navbar'>
                        <button onclick='openEditor()'>Editor</button>
                        <button onclick='window.location.href=""documentacao.html""'>Documentation</button>
                        <button>Code</button>
                        <button onclick='window.location.href=""chat.html""'>Chat</button>
                    </div>
                ";

                string chartHtml = @"
                    <div class='chart-container'>
                        <div class='chart-title'>Flow Velocity by Sector</div>
                        <canvas id='flowChart'></canvas>
                    </div>
                ";

                string chartScript = @"
                    <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
                    <script>
                        document.addEventListener('DOMContentLoaded', function() {
                            const data = {
                                'Sector A': 12.5,
                                'Sector B': 8.2,
                                'Sector C': 15.7,
                                'Sector D': 6.4
                            };

                            new Chart(document.getElementById('flowChart'), {
                                type: 'bar',
                                data: {
                                    labels: Object.keys(data),
                                    datasets: [{
                                        label: 'Velocity (m³/s)',
                                        data: Object.values(data),
                                        backgroundColor: [
                                            'rgba(54, 162, 235, 0.7)',
                                            'rgba(255, 99, 132, 0.7)',
                                            'rgba(255, 206, 86, 0.7)',
                                            'rgba(75, 192, 192, 0.7)'
                                        ],
                                        borderColor: [
                                            'rgba(54, 162, 235, 1)',
                                            'rgba(255, 99, 132, 1)',
                                            'rgba(255, 206, 86, 1)',
                                            'rgba(75, 192, 192, 1)'
                                        ],
                                        borderWidth: 1
                                    }]
                                },
                                options: {
                                    responsive: true,
                                    scales: {
                                        y: {
                                            beginAtZero: true
                                        }
                                    }
                                }
                            });
                        });
                    </script>
                ";

                // Insere os estilos
                content = content.Replace("</head>", styleToAdd + "</head>");
                
                // Adiciona a navbar
                content = Regex.Replace(content, @"<div class='navbar'>.*?</div>", "", RegexOptions.Singleline);
                content = content.Replace("<body>", "<body>" + navbarToAdd);
                
                // Adiciona o gráfico após o container da Unity
                content = content.Replace("</div>\n    <script>", chartHtml + "</div>\n    <script>");
                
                // Adiciona o script do gráfico antes do fechamento do body
                content = content.Replace("</body>", chartScript + "</body>");

                File.WriteAllText(indexPath, content);
                Debug.Log("Gráfico adicionado abaixo do canvas da Unity!");
            }
        }
    }
}
