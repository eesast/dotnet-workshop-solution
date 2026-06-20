import os
import sys

for i in range(1, 31):
    datestr = f"202607{i:02d}"
    count = 40000 + int(datestr) - 20260601
    os.system(f"syspy gen.py -C Dataset{datestr} --csv multiple-logs/{datestr}.log --cs gen_logs/Dataset{datestr}.cs -n TestUtils.dataset.multiple_logs -c {count}")
