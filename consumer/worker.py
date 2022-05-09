import numpy as np
import pandas as pd
import numpy as np
from keras.preprocessing import image

import matplotlib.pyplot as plt

import pika
import time
import pickle 

import asyncpg
import asyncio


connection = pika.BlockingConnection(pika.ConnectionParameters("localhost"))
channel = connection.channel()

# Здесь снова объявляем, что для получения сообщений из этой очереди (вы можете оставить ее, но у вас должна быть эта очередь, иначе вы получите ошибку)
channel.queue_declare(
    queue="MyQueue",
    # Durable = True # durable = True Постоянство для очереди
)

def predict(filename):
    time.sleep(15)
    model_name = '../../exp/finalized_model.sav'

    loaded_model = pickle.load(open(model_name, 'rb'))
    path='../apply_neural_network/wwwroot/pictures_data/' + filename
    img=image.load_img(path, target_size=(150, 150))
    x=image.img_to_array(img)
    x=np.expand_dims(x, axis=0)
    images = np.vstack([x])
    classes = loaded_model.predict(images, batch_size=10)

    if classes[0]>0:
        print(f"it is a dog!")
        return "dog"

    else:
        print(f"it is a cat!")
        return "cat"

async def callback_inner(ch, method, properties, body):
    con = await asyncpg.connect(user='postgres', host='127.0.0.1', password='postgres', port=5432)
    cursor = con
    message = body.decode()
    splitted = message.split('#')

    filename = '#'.join(splitted[1:])
    task_id = splitted[0]

    print("task " + task_id + " started!")
    await cursor.execute(f"""
        INSERT INTO tasks(task_id, status)
        VALUES(\'{task_id}\', \'STARTED\')
        ON CONFLICT(task_id) DO UPDATE
        SET status = excluded.status;
    """)
    try:
        result = predict(message)
    except Exception as e:
        await cursor.execute(f"""
            INSERT INTO tasks(task_id, status)
            VALUES(\'{task_id}\', \'FAILURE\')
                status
            ON CONFLICT(task_id) DO UPDATE
            SET status = excluded.status;
        """)
        print('exception what: ', e)
        return
    
    await cursor.execute(f"""
        INSERT INTO tasks(task_id, status, result)
        VALUES(\'{task_id}\', \'SUCCESS\', \'{result}\')
        ON CONFLICT(task_id) DO UPDATE
        SET status = excluded.status,
            result = excluded.result;
    """)
    print("task " + task_id + " finished!")

def callback(ch, method, properties, body):
    asyncio.get_event_loop().run_until_complete(callback_inner(ch, method, properties, body))
    ch.basic_ack(delivery_tag=method.delivery_tag)

channel.basic_qos(prefetch_count=1)

channel.basic_consume(
    callback,
    queue="MyQueue",
    # No_ack = True # Отменить функцию обработки прерывания отправляющего сообщения, независимо от того, было ли оно обработано, оно не отправит подтверждение на сервер.
)

print("[*] Waiting for messages. To exit press CTRL+C")

channel.start_consuming()