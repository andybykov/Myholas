window.renderChart = (canvasId, labels, data, rawStates, stateMap) => {
    const ctx = document.getElementById(canvasId).getContext('2d');
    const existingChart = Chart.getChart(ctx);
    if (existingChart) existingChart.destroy();

    // Если stateMap передан, значит у нас текстовые значения (select, switch)
    const isCategorical = stateMap && stateMap.length > 0;

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Значение',
                data: data,
                borderColor: 'rgb(75, 192, 199)',
                tension: 0.1,
                fill: true,
                backgroundColor: 'rgba(75, 192, 199, 0.2)',
                // Ступенчатый график для категорий (low -> medium)
                stepped: isCategorical 
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const index = context.dataIndex;
                            // Показываем оригинальную строку из БД
                            return `Состояние: ${rawStates[index]}`;
                        }
                    }
                }
            },
            scales: {
                x: { ticks: { maxRotation: 45, autoSkip: true } },
                y: {
                    beginAtZero: true,
                    ticks: {
                        // Если данные категориальные, заменяем цифры 0,1,2 на "low", "medium" и т.д.
                        callback: function(value) {
                            if (isCategorical && stateMap[value]) {
                                return stateMap[value];
                            }
                            return value;
                        },
                        stepSize: isCategorical ? 1 : undefined
                    }
                }
            }
        }
    });
};
