import { Component, Inject, ViewChild  } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ChartConfiguration, ChartData, ChartEvent, ChartType } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  @ViewChild('panelA') chartA: BaseChartDirective | undefined;
  @ViewChild('panelB') chartB: BaseChartDirective | undefined;

  public barChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    // We use these empty structures as placeholders for dynamic theming.
    scales: {
      x: {},
      y: {
        min: 0
      }
    },
    plugins: {
      tooltip: {
        callbacks: {
          label: (item) =>
            `  ${item.formattedValue} øre`,
        },
      },
    }
  };
  public barChartType: ChartType = 'bar';

  public barChartDataToday: ChartData<'bar'> = {
    labels: [],
    datasets: [
      { data: [], label: '' }
    ]
  };

  public barChartDataTomorrow: ChartData<'bar'> = {
    labels: [],
    datasets: [
      { data: [], label: '' }
    ]
  };


  public elspotData: IElspotData = {
    todayBadTime: "",
    todayBestTime: "",
    todayBestPrice: "",
    todayDay: "",
    tomorrowBadTime: "",
    tomorrowBestTime: "",
    tomorrowDay: "",
    todayBadPrice: "",
    tomorrowBadPrice: "",
    tomorrowBestPrice: "",
    currentPrice: "",
    chartDataToday: {
      labels: [],
      datasets: []
    },
    chartDataTomorrow: {
      labels: [],
      datasets: []
    }
  };

  loading: boolean = false;

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
    this.bindData();
  }

  bindData(): void {
    this.loading = true;
    this.http.get<IElspotData>(this.baseUrl + 'api/elspotprice/getstatus').subscribe(result => {
      if (result) {
        this.elspotData = result;

        this.barChartDataToday.labels = this.elspotData.chartDataToday.labels;
        this.barChartDataToday.datasets = this.elspotData.chartDataToday.datasets;

        this.barChartDataTomorrow.labels = this.elspotData.chartDataTomorrow.labels;
        this.barChartDataTomorrow.datasets = this.elspotData.chartDataTomorrow.datasets;

        this.chartA?.update();
        this.chartB?.update();

      }
      this.loading = false;
    }, error => {
      this.loading = false;
      console.error(error);
    });
  }

  refresh(): void {
    this.bindData();
  }
}

interface IElspotData {
  todayDay: string;
  todayBestTime: string;
  todayBadTime: string;
  tomorrowDay: string;
  tomorrowBestTime: string;
  tomorrowBadTime: string;
  todayBestPrice: string;
  todayBadPrice: string;
  tomorrowBestPrice: string;
  tomorrowBadPrice: string;
  currentPrice: string;
  chartDataToday: IElspotDataChart;
  chartDataTomorrow: IElspotDataChart;
}

interface IElspotDataChart {
  labels: string[];
  datasets: IElspotDataChartData[];
}

interface IElspotDataChartData {
  label: string;
  data: number[];
  backgroundColor: string[];
}
