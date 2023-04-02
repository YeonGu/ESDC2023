package com.example.uhfsdkdemo;

import java.io.Reader;

import com.android.hdhe.uhf.consts.Constants;
import com.android.hdhe.uhf.reader.NewSendCommendManager;
import com.android.hdhe.uhf.reader.UhfReader;

import android.app.Activity;
import android.os.Bundle;
import android.util.Log;
import android.view.ActionMode;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.AdapterView.OnItemSelectedListener;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

public class SettingActivity extends Activity implements OnClickListener{
	private Button button1;//设置按钮1
	private Button button2;//设置按钮2
	private Button button3;//设置按钮1
	private Button button4;//设置按钮2
	private Spinner spinnerSensitive;//灵敏度
	private Spinner spinnerPower;//RF功率
	private Spinner spinnerWorkArea;//工作区域
	private EditText editFrequency;//频率
	private String[] powers = {"26dbm","24dbm","20dbm","18.5dbm","17dbm","15.5dbm","14dbm","12.5dbm"};
	private String[] sensitives = null;
	
	private String[] areas = null;
	private ArrayAdapter<String> adapterSensitive;
	private ArrayAdapter<String> adapterPower;
	private ArrayAdapter<String> adapterArea;
	private UhfReader reader ;
	private int sensitive = 0;
	private int power = 0 ;
	private int area = 0;
	private int frequency = 0;
	private TextView textTips ;
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		setContentView(R.layout.setting_activity);
		super.onCreate(savedInstanceState);
		reader = UhfReader.getInstance();
		initView();
	}
		
	private void initView(){
		button1 = (Button) findViewById(R.id.button_min);
		button2 = (Button) findViewById(R.id.button_plus);
		button3 = (Button) findViewById(R.id.button_set);
		button4 = (Button) findViewById(R.id.button4);
		
		textTips = (TextView) findViewById(R.id.textViewTips);
		spinnerSensitive = (Spinner) findViewById(R.id.spinner1);
		spinnerPower = (Spinner) findViewById(R.id.spinner2);
		spinnerWorkArea = (Spinner) findViewById(R.id.spinner3);
		editFrequency = (EditText) findViewById(R.id.edit4);
		sensitives = getResources().getStringArray(R.array.arr_sensitivity);
		areas = getResources().getStringArray(R.array.arr_area);
		
		adapterSensitive = new  ArrayAdapter<String>(this, android.R.layout.simple_spinner_dropdown_item, sensitives);
		adapterPower = new ArrayAdapter<String>(this, android.R.layout.simple_spinner_dropdown_item, powers);
		adapterArea = new ArrayAdapter<String>(this	, android.R.layout.simple_dropdown_item_1line, areas);
		spinnerSensitive.setAdapter(adapterSensitive);
		spinnerPower.setAdapter(adapterPower);
		spinnerWorkArea.setAdapter(adapterArea);
		button1.setOnClickListener(this);
		button2.setOnClickListener(this);
		button3.setOnClickListener(this);
		button4.setOnClickListener(this);
		spinnerWorkArea.setOnItemSelectedListener(new OnItemSelectedListener() {

			@Override
			public void onItemSelected(AdapterView<?> adapter, View view,
					int position, long id) {
//				if(position == 5){
//					area = position + 1;
//				}else{
//					area = position ;
//				}
				switch (position) {
				case 0:
					area = 1;
					textTips.setText(R.string.china2Freq);
					break;
				case 1:
					area = 2;
					textTips.setText(R.string.usaFreq);
					break;
				case 2:
					area = 3;
					textTips.setText(R.string.euFreq);
					break;
				case 3:
					area = 4;
					textTips.setText(R.string.china1Freq);
					break;
				case 4:
					area = 6;
					textTips.setText(R.string.koreaFreq);
					break;

				default:
					break;
				}
				
			}

			@Override
			public void onNothingSelected(AdapterView<?> arg0) {
				// TODO Auto-generated method stub
				
			}
		});
		spinnerSensitive.setOnItemSelectedListener(new OnItemSelectedListener() {

			@Override
			public void onItemSelected(AdapterView<?> arg0, View arg1,
					int position, long arg3) {
				
				Log.e("", sensitives[position]);
				switch (position) {
				case 0:
					sensitive = NewSendCommendManager.SENSITIVE_HIHG;
					break;
				case 1:
					sensitive = NewSendCommendManager.SENSITIVE_MIDDLE;
					break;
				case 2:
					sensitive = NewSendCommendManager.SENSITIVE_LOW;
					break;
				case 3:
					sensitive = NewSendCommendManager.SENSITIVE_VERY_LOW;
					break;

				default:
					break;
				}
			}

			@Override
			public void onNothingSelected(AdapterView<?> arg0) {
				// TODO Auto-generated method stub
				
			}
		});
		spinnerPower.setOnItemSelectedListener(new OnItemSelectedListener() {

			@Override
			public void onItemSelected(AdapterView<?> arg0, View arg1,
					int position, long arg3) {
				Log.e("", powers[position]);
				switch (position) {
				case 0:
					power = 2600;
					break;
				case 1:
					power =2400;
					break;
				case 2:
					power = 2000;
					break;
				case 3:
					power = 1850;
					break;
				case 4:
					power = 1700;
					break;
				case 5:
					power = 1550;
					break;
				case 6:
					power = 1400;
					break;
				case 7:
					power = 1250;
					break;

				default:
					break;
				}
				
			}

			@Override
			public void onNothingSelected(AdapterView<?> arg0) {
				// TODO Auto-generated method stub
				
			}
		});
	}

	@Override
	public void onClick(View v) {
		Log.e("", "sensitive = " + sensitive+ "; power =  " + power);
		switch (v.getId()) {
		case R.id.button_min:
			reader.setSensitivity(sensitive);
			Toast.makeText(getApplicationContext(), R.string.setSuccess, 0).show();
			break;
		case R.id.button_plus:
			reader.setOutputPower(power);
			Toast.makeText(getApplicationContext(), R.string.setSuccess, 0).show();
			break;
		case R.id.button_set:
//			reader.setWorkArea(area);
			Toast.makeText(getApplicationContext(), R.string.setSuccess, 0).show();
			break;
		case R.id.button4:
			String freqStr = editFrequency.getText().toString();
			if(freqStr == null || "".equals(freqStr)){
				Toast.makeText(getApplicationContext(), R.string.freqSetting, 0).show();
				return;
			}
//			reader.setFrequency(frequency, 0, 0);
			Toast.makeText(getApplicationContext(), R.string.setSuccess, 0).show();
			break;

		default:
			break;
		}
		
	}

//	@Override
//	public void onItemSelected(AdapterView<?> adapter, View view, int position, long id) {
//		
//		Log.e("", sensitives[position]);
//		switch (view.getId()) {
//		
//		case R.id.spinner1://灵敏度
//			Log.e("", sensitives[position]);
//			break;
//		case R.id.spinner2://RF功率
//			Log.e("", powers[position]);
//		break;
//
//	default:
//		break;
//	}
//	
//	}
//
//	@Override
//	public void onNothingSelected(AdapterView<?> arg0) {
//		// TODO Auto-generated method stub
//		
//	}

//	@Override
//	public void (AdapterView<?> adapter, View view, int position, long id) {
//		switch (view.getId()) {
//		case R.id.spinner1://灵敏度
//			Log.e("", sensitives[position]);
//			break;
//		case R.id.spinner2://RF功率
//			Log.e("", powers[position]);
//			break;
//
//		default:
//			break;
//		}
//		
//	}
}
