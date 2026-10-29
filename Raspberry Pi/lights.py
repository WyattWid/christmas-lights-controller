import serial
import time
import vlc

# ---------- Serial Setup ----------
ser = serial.Serial(
    '/dev/ttyACM0',   # adjust if needed
    230400,
    timeout=0
)
time.sleep(20)
txt_files = ['rockin.txt', 'grinch.txt']
song_files = ['rockin.wav', 'grinch.wav']
while True:
    for song_num in range(len(txt_files)):
        print("opening file")
        # ---------- Load LED Patterns ----------
        pattern_file = txt_files[song_num]
    
        with open(pattern_file, 'r') as f:
            lines = [line.strip() for line in f if line.strip()]
        
        NUM_STRANDS = 16
        STEP_TIME = 0.1  # seconds per step
        
        if len(lines) < NUM_STRANDS:
            lines += ['0' * len(lines[0])] * (NUM_STRANDS - len(lines))
        
        total_steps = max(len(line) for line in lines)-1
        lines = [line.ljust(total_steps, '0') for line in lines]
        
        # Precompute frames (important for CPU)
        frames = [
            ''.join(lines[i][step] for i in range(NUM_STRANDS))
            for step in range(total_steps)
        ]
        
        # ---------- Audio Setup (VLC) ----------
        player = vlc.MediaPlayer(song_files[song_num])
        media = player.get_media()
        
        media.parse()
        time.sleep(0.1)
        
        length = media.get_duration() / 1000.0
        player.play()
        
        
        # Give VLC a moment to start
        time.sleep(0.3)
        
    
        # ---------- Main Loop ----------
        last_step = -1
        poll_count = 0
        poll_interval = 75
        loop_start = time.time()
        confirmed_time = player.get_time() / 1000.0
        while True:
    
            # VLC time is in ms
            elapsed = time.time() - (confirmed_time + loop_start)
            audio_time = (confirmed_time + elapsed)
            #print("time: "+str(time.time()))
            #print("confirmed time: "+str(confirmed_time))
            #print("loop: "+str(loop_start))
            #print("elapsed time: "+str(elapsed))
            #print("audio time: "+str(audio_time))
            
            step_idx = int(audio_time / STEP_TIME)
            if step_idx >= total_steps or audio_time >= length:
                print("break")
                break
            audio_time = audio_time + 0.02
            if step_idx != last_step and 0 <= step_idx < total_steps:
                ser.write((frames[step_idx] + '\n').encode())
                ser.flush()
                last_step = step_idx
            
            poll_count += 1
        
            if poll_count >= poll_interval:
                current_vlc_time = player.get_time() / 1000.0
                if current_vlc_time != 1:
                    # resync safely — only forward
                    confirmed_time = (current_vlc_time)
                    print("confirm: " + str(confirmed_time))
                else:
                    print("sync failed")
                    break
                poll_count = 0
            
            # Sleep enough to avoid CPU burn but keep sync tight
            time.sleep(0.01)
        
