﻿using System;
using System.IO;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.Audio.Mci;
using NWaves.Effects;
using NWaves.Filters.Base;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.Transforms;

namespace NWaves.DemoForms
{
    public partial class EffectsForm : Form
    {
        private DiscreteSignal _signal;
        private DiscreteSignal _filteredSignal;

        private readonly Stft _stft = new Stft(256, fftSize: 256);

        private string _waveFileName;

        private readonly MciAudioPlayer _player = new MciAudioPlayer();


        public EffectsForm()
        {
            InitializeComponent();

            signalBeforeFilteringPanel.Gain = 80;
            signalAfterFilteringPanel.Gain = 80;
        }
        
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _waveFileName = ofd.FileName;

            using (var stream = new FileStream(_waveFileName, FileMode.Open))
            {
                var waveFile = new WaveFile(stream);
                _signal = waveFile[Channels.Average];
            }

            signalBeforeFilteringPanel.Signal = _signal;
            spectrogramBeforeFilteringPanel.Spectrogram = _stft.Spectrogram(_signal);
        }

        private void applyEffectButton_Click(object sender, EventArgs e)
        {
            IFilter effect;

            if (tremoloRadioButton.Checked)
            {
                var freq = double.Parse(tremoloFrequencyTextBox.Text);
                var index = double.Parse(tremoloIndexTextBox.Text);
                effect = new TremoloEffect(freq, index);
            }
            else if (overdriveRadioButton.Checked)
            {
                effect = new OverdriveEffect();
            }
            else if (distortionRadioButton.Checked)
            {
                var gain = double.Parse(distortionGainTextBox.Text);
                var mix = double.Parse(distortionMixTextBox.Text);
                effect = new DistortionEffect(gain, mix);
            }
            else if (tubeDistortionRadioButton.Checked)
            {
                var gain = double.Parse(distortionGainTextBox.Text);
                var mix = double.Parse(distortionMixTextBox.Text);
                var dist = double.Parse(distTextBox.Text);
                var q = double.Parse(qTextBox.Text);
                effect = new TubeDistortionEffect(gain, mix, q, dist);
            }
            else if (echoRadioButton.Checked)
            {
                var delay = double.Parse(echoDelayTextBox.Text);
                var decay = double.Parse(echoDecayTextBox.Text);
                effect = new EchoEffect(delay, decay);
            }
            else if (delayRadioButton.Checked)
            {
                var delay = double.Parse(echoDelayTextBox.Text);
                var decay = double.Parse(echoDecayTextBox.Text);
                effect = new DelayEffect(delay, decay);
            }
            else if (wahwahRadioButton.Checked)
            {
                var lfoFrequency = double.Parse(lfoFreqTextBox.Text);
                var minFrequency = double.Parse(minFreqTextBox.Text);
                var maxFrequency = double.Parse(maxFreqTextBox.Text);
                var q = double.Parse(lfoQTextBox.Text);
                effect = new WahwahEffect(lfoFrequency, minFrequency, maxFrequency, q);
            }
            else if (pitchShiftRadioButton.Checked)
            {
                var shift = double.Parse(pitchShiftTextBox.Text);
                effect = pitchShiftCheckBox.Checked ? new PitchShiftEffect(shift) : null;
            }
            else
            {
                var lfoFrequency = double.Parse(lfoFreqTextBox.Text);
                var minFrequency = double.Parse(minFreqTextBox.Text);
                var maxFrequency = double.Parse(maxFreqTextBox.Text);
                var q = double.Parse(lfoQTextBox.Text);
                effect = new PhaserEffect(lfoFrequency, minFrequency, maxFrequency, q);
            }

            _filteredSignal = effect != null ? 
                              effect.ApplyTo(_signal, FilteringOptions.Auto) : 
                              Operation.TimeStretch(_signal, double.Parse(pitchShiftTextBox.Text));

            signalAfterFilteringPanel.Signal = _filteredSignal;
            spectrogramAfterFilteringPanel.Spectrogram = _stft.Spectrogram(_filteredSignal.Samples);
        }

        #region playback

        private async void playSignalButton_Click(object sender, EventArgs e)
        {
            await _player.PlayAsync(_waveFileName);
        }

        private async void playFilteredSignalButton_Click(object sender, EventArgs e)
        {
            // create temporary file
            const string tmpFilename = "tmpfiltered.wav";
            using (var stream = new FileStream(tmpFilename, FileMode.Create))
            {
                var waveFile = new WaveFile(_filteredSignal);
                waveFile.SaveTo(stream);
            }

            await _player.PlayAsync(tmpFilename);
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            _player.Stop();
        }

        #endregion
    }
}