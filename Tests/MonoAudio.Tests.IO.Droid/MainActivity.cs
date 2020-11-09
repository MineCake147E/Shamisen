#region Using Declaration

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Net;
using AndroidX.Core.App;
using DivideSharp;
using MonoAudio;
using MonoAudio.Conversion.Resampling.Sample;
using MonoAudio.Conversion.SampleToWaveConverters;
using MonoAudio.Filters;
using MonoAudio.IO;
using MonoAudio.IO.Android;
using MonoAudio.Synthesis;
using Debug = System.Diagnostics.Debug;
using Uri = Android.Net.Uri;

#endregion Using Declaration

namespace MonoAudio.Tests.IO.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private const double Freq = 523.2511306011972693556999870466094027289077206840796617283 * 4;
        private const int PermissionRequestCode = 3939;
        private const int FileWriteRequestCode = 1515;
        private const int Channels = 2;
        private const int SampleRate = 23000;
        private const int DestinationSampleRate = 192000;
        private const float Scale = 0.5f;
        private AudioTrackOutput ato;
        private SplineResampler resampler;
        private SinusoidSource sinusoid;
        private Attenuator attenuator;
        private SampleToWaveConverterBase converter;
        private TextView dump;
        private static readonly string TAG = "Permission";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            dump = FindViewById<TextView>(Resource.Id.Dump);
            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
            /*var requiredPermissions = new string[] { Manifest.Permission.WriteExternalStorage };
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessFineLocation))
            {
                // Provide an additional rationale to the user if the permission was not granted
                // and the user would benefit from additional context for the use of the permission.
                // For example if the user has previously denied the permission.
                Log.Info("INFO", "Displaying camera permission rationale to provide additional context.");

                Snackbar.Make(Window.DecorView, Resource.String.perm_ewrite, Snackbar.LengthIndefinite)
                    .SetAction(Resource.String.ok, view => ActivityCompat.RequestPermissions(this, requiredPermissions, PermissionRequestCode)).Show();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, requiredPermissions, PermissionRequestCode);
            }*/
            InitializeOutput();
            WriteLineToDump($"SIMD Length:{Vector<float>.Count}");
            WriteLineToDump($"Vector.IsHardwareAccelerated:{Vector.IsHardwareAccelerated}");
        }

        private void InitializeOutput()
        {
            ato = new AudioTrackOutput(AudioUsageKind.Game, AudioContentType.Music, TimeSpan.FromMilliseconds(128));
            sinusoid = new SinusoidSource(new SampleFormat(Channels, SampleRate)) { Frequency = Freq };
            resampler = new SplineResampler(sinusoid, DestinationSampleRate);
            attenuator = new Attenuator(resampler) { Scale = Scale };
            converter = new SampleToPcm16Converter(attenuator, true);
            ato.Initialize(converter);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                var intent = new Intent(Intent.ActionCreateDocument);
                intent.AddCategory(Intent.CategoryOpenable);
                intent.SetFlags(ActivityFlags.GrantWriteUriPermission);
                intent.SetFlags(ActivityFlags.GrantReadUriPermission);
                intent.SetFlags(ActivityFlags.NewTask);
                intent.SetFlags(ActivityFlags.ClearWhenTaskReset);
                intent.SetType("audio/*");
                intent.PutExtra(Intent.ExtraTitle, $"SplineResamplerDump_{Channels}ch_{SampleRate}to{DestinationSampleRate}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_fffffff}.wav");
                StartActivityForResult(intent, FileWriteRequestCode);
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        protected override async void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            switch (requestCode)
            {
                case FileWriteRequestCode:
                    if (resultCode == Result.Ok)
                    {
                        //OKボタンを押して戻ってきたときの処理
                        var path = data.Data;
                        Log.Verbose("Path", path.ToString());
                        WriteLineToDump(await Task.Run(() => EncodeDump(path)));
                    }
                    else if (resultCode == Result.Canceled)
                    {
                        //キャンセルボタンを押して戻ってきたときの処理
                    }
                    else
                    {
                        //その他
                    }
                    break;
                default:
                    base.OnActivityResult(requestCode, resultCode, data);
                    break;
            }
        }

        private string EncodeDump(Uri path)
        {
            try
            {
                var sb = new StringBuilder();
                var sin = new SinusoidSource(new SampleFormat(Channels, SampleRate)) { Frequency = Freq };
                var resam = new SplineResampler(sin, DestinationSampleRate);
                var vol = new Attenuator(resam) { Scale = Scale };
                var conv = new SampleToFloat32Converter(vol);
                var buffer = new byte[192 * conv.Format.GetFrameSizeInBytes()];
                var lenToWrite = TimeSpan.FromSeconds(10);
                var bytesToWtite = conv.Format.GetBufferSizeRequired(lenToWrite);
                var blocksToWrite = bytesToWtite / buffer.Length;
                sb.AppendLine(path.ToString());
                using (var st = ContentResolver.OpenOutputStream(path))
                {
                    //Needed to write with WAVE format so implementing temporary encoder
                    Span<byte> bytebuf = stackalloc byte[] {
                        0x52,
                        0x49,
                        0x46,
                        0x46, //RIFF
                        0xff,
                        0xff,
                        0xff,
                        0xff, //ckSize
                        0x57,
                        0x41,
                        0x56,
                        0x45, //WAVE
                        0x66,
                        0x6D,
                        0x74,
                        0x20, //fmt_
                        0x12,
                        0x00,
                        0x00,
                        0x00, //ckSize
                        0xff,
                        0xff,             //wFormatTag
                        0xff,
                        0xff,             //nChannels
                        0xff,
                        0xff,
                        0xff,
                        0xff, //nSamplesPerSec
                        0xff,
                        0xff,
                        0xff,
                        0xff, //nAvgBytesPerSec
                        0xff,
                        0xff,             //nBlockAlign
                        0xff,
                        0xff,             //wBitsPerSample
                        0x00,
                        0x00,             //Mysterious Trailing zeros
                        0x64,
                        0x61,
                        0x74,
                        0x61, //data
                        0xff,
                        0xff,
                        0xff,
                        0xff  //ckSize
                    };
                    BinaryPrimitives.WriteInt32LittleEndian(bytebuf.Skip(4), bytebuf.Length - 8 + bytesToWtite);    //cksize of RIFF
                    BinaryPrimitives.WriteInt16LittleEndian(bytebuf.Skip(20), 3);   //wFormatTag
                    BinaryPrimitives.WriteInt16LittleEndian(bytebuf.Skip(22), (short)conv.Format.Channels);   //nChannels
                    BinaryPrimitives.WriteInt32LittleEndian(bytebuf.Skip(24), conv.Format.SampleRate);   //nSamplesPerSec
                    BinaryPrimitives.WriteInt32LittleEndian(bytebuf.Skip(28),
                        conv.Format.GetBufferSizeRequired(TimeSpan.FromSeconds(1)));   //nAvgBytesPerSec
                    BinaryPrimitives.WriteInt16LittleEndian(bytebuf.Skip(32), (short)conv.Format.GetFrameSizeInBytes());   //nBlockAlign
                    BinaryPrimitives.WriteInt16LittleEndian(bytebuf.Skip(34), (short)conv.Format.BitDepth);   //wBitsPerSample
                    BinaryPrimitives.WriteInt32LittleEndian(bytebuf.Skip(42), bytesToWtite);   //wBitsPerSample
                    st.Write(bytebuf);
                    for (int i = 0; i < blocksToWrite; i++)
                    {
                        var rr = conv.Read(buffer);
                        if (rr.HasData)
                        {
                            st.Write(buffer);
                        }
                    }
                    st.Flush();
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        private void WriteLineToDump(string value) => dump.Text += value + "\n";

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            var view = (View)sender;
            switch (ato?.PlaybackState ?? PlaybackState.NotInitialized)
            {
                case PlaybackState.NotInitialized:
                    ato?.Dispose();
                    InitializeOutput();
                    break;
                case PlaybackState.Stopped:
                    ato.Play();
                    break;
                case PlaybackState.Playing:
                    ato.Pause();
                    break;
                case PlaybackState.Paused:
                    ato.Resume();
                    break;
                default:
                    throw new InvalidOperationException($"Invalid state {ato.PlaybackState}!");
            }
            Snackbar.Make(view, $"Output is now {ato?.PlaybackState}", Snackbar.LengthLong)
                .SetAction("Action", (View.IOnClickListener)null).Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == PermissionRequestCode)
            {
                // Received permission result for camera permission.
                Log.Info(TAG, "Received response for Location permission request.");
            }
            else
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }
    }
}
