
```java
Button btn01;
private Uri fileURI;

@Override
public void onCreate(Bundle savedInstanceState) {
    super.onCreate(savedInstanceState);
    setContentView(R.layout.main);

    btn01 = (Button) findViewById(R.id.btn01);
    btn01.setOnClickListener(new OnClickListener() {

        @Override
        public void onClick(View v) {
            // TODO Auto-generated method stub

            Intent intenet = new Intent(MediaStore.ACTION_IMAGE_CAPTURE);
            fileURI = getoutputmediafileuri();
            //intenet.putExtra("output", uri.getPath());
            startActivityForResult(intenet,0);
        }
    });
}
```
